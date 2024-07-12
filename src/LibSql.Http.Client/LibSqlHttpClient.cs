using System.Net.Http.Headers;
using System.Text.Json.Serialization.Metadata;
using LibSql.Http.Client.Exceptions;
using LibSql.Http.Client.Interfaces;
using LibSql.Http.Client.Request;
using LibSql.Http.Client.Response;

namespace LibSql.Http.Client;

/// <inheritdoc />
public sealed class LibSqlHttpClient : ILibSqlHttpClient
{
    private const string PipelineV3Path = "/v3/pipeline";
    private readonly AuthenticationHeaderValue? _authHeaderValue;

    private readonly HttpClient _httpClient;
    private readonly Uri _pipelineUri;

    /// <summary>
    ///     Creates a new instance of <see cref="LibSqlHttpClient" />.
    /// </summary>
    /// <param name="httpClient">HttpClient instance to use</param>
    /// <param name="url">
    ///     Specify a LibSQL url to use if a singleton http client is being used among different clients or is a
    ///     multi db app
    /// </param>
    /// <param name="authToken">Authorization token</param>
    /// <exception cref="UriFormatException">The URI is not valid</exception>
    /// <exception cref="ArgumentNullException">Neither HttpClient BaseAddress nor URL param is set</exception>
    /// <example>
    ///     The recommended approach according to HTTP Client guidelines is a singleton HTTP Client instance per app
    ///     <code>
    ///         var handler = new SocketsHttpHandler
    ///         {
    ///             PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Recreate every 15 minutes
    ///         };
    ///         var sharedClient = new HttpClient(handler);
    ///         var libSqlClient = new LibSqlHttpClient(sharedClient, new Uri("https://db.host.com"), "YOUR_AUTH_TOKEN");
    ///     </code>
    ///     or
    ///     <code>
    ///         var handler = new SocketsHttpHandler
    ///         {
    ///             PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Recreate every 15 minutes
    ///         };
    ///         var sharedClient = new HttpClient(handler)
    ///         {
    ///             BaseAddress = new Uri("https://db.host.com/"),
    ///             DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_AUTH_TOKEN" )}
    ///         };
    ///         var libSqlClient = new LibSqlHttpClient(sharedClient);
    ///     </code>
    ///     or
    ///     <code>
    ///         builder.Services.AddHttpClient&lt;ILibSqlHttpClient, LibSqlHttpClient&gt;(
    ///             client =>
    ///             {
    ///                 client.BaseAddress = new Uri("https://db.host.com/");
    ///                 client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_AUTH_TOKEN");
    ///             });
    ///     </code>
    /// </example>
    public LibSqlHttpClient(HttpClient httpClient, Uri? url = null, string? authToken = null)
    {
        url ??= httpClient.BaseAddress;

        if (url is null)
            throw new ArgumentNullException(
                nameof(url),
                "URL not set. Please provide a URL either in the constructor or as a parameter or via HttpClient.BaseAddress.");

        _pipelineUri = new Uri(url, PipelineV3Path);

        _httpClient = httpClient;

        if (authToken is not null)
            _authHeaderValue = new AuthenticationHeaderValue("Bearer", authToken.Replace("Bearer ", ""));
    }

    /// <inheritdoc />
    public ILibSqlHttpClient WithCredentials(Uri url, string? authToken = null) =>
        new LibSqlHttpClient(_httpClient, url, authToken);

    /// <inheritdoc />
    public Task<int> ExecuteAsync(
        Statement statement,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default) =>
        ExecuteMultipleAsync(
            [statement],
            transactionMode,
            cancellationToken);

    /// <inheritdoc />
    public Task<int> ExecuteMultipleAsync(
        Statement[] statements,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default) =>
        InternalSendPipelineRequestAsync(
            statements,
            transactionMode,
            reader => reader.AffectedRows,
            true,
            cancellationToken);

    /// <inheritdoc />
    public Task<object?> ExecuteScalarAsync(
        Statement statement,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default) =>
        InternalSendPipelineRequestAsync(
            [statement],
            transactionMode,
            reader => reader.GetScalarValue(),
            true,
            cancellationToken);

    /// <inheritdoc />
    public Task<T> QueryFirstAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default) =>
        InternalQueryAsync(
            statement,
            jsonTypeInfo,
            transactionMode,
            result => result.First(),
            cancellationToken);

    /// <inheritdoc />
    public Task<T?> QueryFirstOrDefaultAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default) =>
        InternalQueryAsync(
            statement,
            jsonTypeInfo,
            transactionMode,
            result => result.FirstOrDefault(),
            cancellationToken);

    /// <inheritdoc />
    public Task<T> QuerySingleAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default) =>
        InternalQueryAsync(
            statement,
            jsonTypeInfo,
            transactionMode,
            result => result.Single(),
            cancellationToken);

    /// <inheritdoc />
    public Task<T?> QuerySingleOrDefaultAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default) =>
        InternalQueryAsync(
            statement,
            jsonTypeInfo,
            transactionMode,
            result => result.SingleOrDefault(),
            cancellationToken);

    /// <inheritdoc />
    public Task<IEnumerable<T>> QueryAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default) =>
        InternalQueryAsync(
            statement,
            jsonTypeInfo,
            transactionMode,
            result => result,
            cancellationToken);

    /// <inheritdoc />
    public Task<IResultReader> QueryMultipleAsync(
        Statement[] statements,
        TransactionMode transactionMode,
        CancellationToken cancellationToken = default) =>
        InternalSendPipelineRequestAsync(statements, transactionMode, reader => reader, false, cancellationToken);

    private Task<TResult> InternalQueryAsync<T, TResult>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode,
        Func<IEnumerable<T>, TResult> processorFn,
        CancellationToken cancellationToken) =>
        InternalSendPipelineRequestAsync(
            [statement],
            transactionMode,
            reader => processorFn(reader.Count > 0 ? reader.ReadAt(0, jsonTypeInfo) : Array.Empty<T>()),
            true,
            cancellationToken);

    private async Task<TResult> InternalSendPipelineRequestAsync<TResult>(
        Statement[] statements,
        TransactionMode transactionMode,
        Func<IResultReader, TResult> processorFn,
        bool disposeReader,
        CancellationToken cancellationToken)
    {
        using var content = RequestSerializer.Serialize(statements, transactionMode);

        using var res = await SendRequestAsync(content, cancellationToken);

        var resultsToIgnore = transactionMode is TransactionMode.None
            ? new HashSet<int>()
            : new HashSet<int> { 0, statements.Length + 1, statements.Length + 2 };

        var reader = await ResultReader.ParseAsync(res.Content, resultsToIgnore, cancellationToken);

        try
        {
            reader.ThrowIfError();

            var result = processorFn(reader);

            return result;
        }
        finally
        {
            if (disposeReader) reader.Dispose();
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpContent content, CancellationToken cancellationToken)
    {
        HttpResponseMessage? res = null;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _pipelineUri)
            {
                Content = content,
                Version = new Version(2, 0)
            };

            if (_authHeaderValue is not null)
                request.Headers.Authorization = _authHeaderValue;

            res = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch (Exception e)
        {
            throw new LibSqlClientException("[LibSqlHttpClient] Error sending pipeline request", e);
        }

        if (res.IsSuccessStatusCode) return res;

        var bodyContent = await res.Content.ReadAsStringAsync(cancellationToken);

        throw new LibSqlClientException(
            $"[LibSqlHttpClient] Error sending pipeline request. Status Code: {res.StatusCode}, body: {bodyContent}");
    }
}
