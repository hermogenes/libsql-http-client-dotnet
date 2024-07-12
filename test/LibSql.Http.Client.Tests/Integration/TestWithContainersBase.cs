using System.Net;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using LibSql.Http.Client.Tests.Integration.Fixture;

namespace LibSql.Http.Client.Tests.Integration;

public abstract class TestWithContainersBase : IAsyncDisposable
{
    private readonly HttpMessageHandler _handler = new SocketsHttpHandler
        { AutomaticDecompression = DecompressionMethods.All };

    private readonly IContainer _libSqlContainer;
    protected readonly ProductTestData TestData;
    private HttpClient _httpClient = new();
    protected LibSqlHttpClient LibSqlClient;

    protected TestWithContainersBase(string tableName)
    {
        TestData = new ProductTestData(tableName);
        ConsoleLogger.Instance.DebugLogLevelEnabled = true;
        LibSqlClient = new LibSqlHttpClient(_httpClient, new Uri("https://fake.test"));
        _libSqlContainer =
            new ContainerBuilder().WithImage("ghcr.io/tursodatabase/libsql-server:latest").WithPortBinding(8080, true)
                .WithWaitStrategy(
                    Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(8080)))
                .Build();
    }

    public async ValueTask DisposeAsync()
    {
        await _libSqlContainer.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected virtual async Task InitializeContainer()
    {
        if (_libSqlContainer.State is TestcontainersStates.Running) return;

        await _libSqlContainer.StartAsync();

        _httpClient = new HttpClient(_handler)
        {
            BaseAddress = new UriBuilder(
                Uri.UriSchemeHttp,
                _libSqlContainer.Hostname,
                _libSqlContainer.GetMappedPublicPort(8080)).Uri
        };

        LibSqlClient = new LibSqlHttpClient(_httpClient);

        await LibSqlClient.ExecuteAsync(TestData.CreateTableSql);
    }
}
