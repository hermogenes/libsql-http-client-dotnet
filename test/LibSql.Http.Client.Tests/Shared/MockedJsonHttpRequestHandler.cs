using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LibSql.Http.Client.Tests.Shared;

public record MockedJsonHttpRequest(HttpMethod Method, string? Uri, string? Token, string? Body = null);

public class MockedJsonHttpResponse(HttpStatusCode statusCode, object? body)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public Task<HttpContent> GetContentAsync() => Task.FromResult<HttpContent>(JsonContent.Create(body));

    public static implicit operator MockedJsonHttpResponse(HttpStatusCode statusCode) => new(statusCode, null);
    public static implicit operator MockedJsonHttpResponse(JsonElement body) => new(HttpStatusCode.OK, body);
}

public class MockedJsonHttpHandler(MockedJsonHttpResponse expectedResponse) : HttpMessageHandler
{
    public static readonly Uri DefaultBaseAddress = new("https://libsql.test/");
    private readonly List<MockedJsonHttpRequest> _sentRequests = [];

    public IReadOnlyList<MockedJsonHttpRequest> SentRequests => _sentRequests;

    public MockedJsonHttpRequest? LastSentRequest => _sentRequests.LastOrDefault();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _sentRequests.Add(
            new MockedJsonHttpRequest(
                request.Method,
                request.RequestUri?.ToString(),
                request.Headers.Authorization?.ToString(),
                request.Content is not null ? await request.Content.ReadAsStringAsync(cancellationToken) : null));

        var content = await expectedResponse.GetContentAsync();

        return new HttpResponseMessage(expectedResponse.StatusCode)
        {
            Content = content
        };
    }

    public static implicit operator HttpClient(MockedJsonHttpHandler handler) => new(handler)
        { BaseAddress = DefaultBaseAddress };
}
