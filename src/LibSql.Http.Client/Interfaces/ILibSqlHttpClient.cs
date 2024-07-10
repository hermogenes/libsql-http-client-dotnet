using System.Text.Json.Serialization.Metadata;
using LibSql.Http.Client.Request;

namespace LibSql.Http.Client.Interfaces;

/// <summary>
///     Provides an interface for executing SQL commands and queries via HTTP, supporting both synchronous and asynchronous
///     operations.
/// </summary>
public interface ILibSqlHttpClient
{
    /// <summary>
    ///     Configures the client with credentials for authentication.
    /// </summary>
    /// <param name="url">The base URL of the SQL HTTP service.</param>
    /// <param name="authToken">An optional authentication token for secured access.</param>
    /// <returns>An instance of the SQL HTTP client configured with specified credentials.</returns>
    /// <remarks>The new instance MUST be used</remarks>
    ILibSqlHttpClient WithCredentials(Uri url, string? authToken = null);

    /// <summary>
    ///     Executes a SQL command asynchronously and returns the number of rows affected.
    /// </summary>
    /// <param name="statement">Statement to execute</param>
    /// <param name="transactionMode">The transaction mode, specifying how the command interacts with transactions.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the number of affected rows as the result.</returns>
    Task<int> ExecuteAsync(
        Statement statement,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes multiple SQL statements asynchronously in a single network call and returns the total number of rows
    ///     affected.
    /// </summary>
    /// <param name="statements">An array of SQL statements to be executed.</param>
    /// <param name="transactionMode">Specifies the transaction mode for the batch operation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the total number of affected rows as the result.</returns>
    Task<int> ExecuteMultipleAsync(
        Statement[] statements,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes a SQL command asynchronously and retrieves the first column of the first row in the result set returned by
    ///     the query.
    /// </summary>
    /// <param name="statement"></param>
    /// <param name="transactionMode">Specifies the transaction mode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the result object of the first column in the first row.</returns>
    Task<object?> ExecuteScalarAsync(
        Statement statement,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Queries the database asynchronously for the first record and maps it to an object of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="statement"></param>
    /// <param name="jsonTypeInfo">Metadata information for JSON serialization and deserialization.</param>
    /// <param name="transactionMode">Specifies the transaction mode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the single queried object as the result.</returns>
    Task<T> QueryFirstAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Queries the database asynchronously for the first record or a default value if no records are found, and maps the
    ///     result to an object of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="statement"></param>
    /// <param name="jsonTypeInfo">Metadata information for JSON serialization and deserialization.</param>
    /// <param name="transactionMode">Specifies the transaction mode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the result object of the first record or a default value.</returns>
    Task<T?> QueryFirstOrDefaultAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Queries the database asynchronously for a single record and maps the
    ///     result to an object of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="statement"></param>
    /// <param name="jsonTypeInfo">Metadata information for JSON serialization and deserialization.</param>
    /// <param name="transactionMode">Specifies the transaction mode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the result object.</returns>
    Task<T> QuerySingleAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Queries the database asynchronously for a single record or a default value if no records are found and maps the
    ///     result to an object of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="statement"></param>
    /// <param name="jsonTypeInfo">Metadata information for JSON serialization and deserialization.</param>
    /// <param name="transactionMode">Specifies the transaction mode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the result object of the first record or a default value.</returns>
    Task<T?> QuerySingleOrDefaultAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Queries the database asynchronously maps the result to a sequence of data of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="statement"></param>
    /// <param name="jsonTypeInfo">Metadata information for JSON serialization and deserialization.</param>
    /// <param name="transactionMode">Specifies the transaction mode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the sequence of values.</returns>
    Task<IEnumerable<T>> QueryAsync<T>(
        Statement statement,
        JsonTypeInfo<T> jsonTypeInfo,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Execute multiple queries on the database asynchronously and return a forward-only result reader.
    /// </summary>
    /// <param name="statements">Queries to execute</param>
    /// <param name="transactionMode">Specifies the transaction mode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the result object of the first record or a default value.</returns>
    Task<IResultReader> QueryMultipleAsync(
        Statement[] statements,
        TransactionMode transactionMode = TransactionMode.None,
        CancellationToken cancellationToken = default);
}
