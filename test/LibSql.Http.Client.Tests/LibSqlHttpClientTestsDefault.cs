using System.Net;
using System.Text.Json;
using FluentAssertions;
using LibSql.Http.Client.Exceptions;
using LibSql.Http.Client.Request;
using LibSql.Http.Client.Tests.Shared;
using LibSql.Http.Client.Tests.Shared.Attributes;
using LibSql.Http.Client.Tests.Shared.Models;

namespace LibSql.Http.Client.Tests;

public class LibSqlHttpClientTestsDefault
{
    [Fact]
    public void ConstructorShouldFailIfUrlIsNull()
    {
        var action = () => new LibSqlHttpClient(new HttpClient());

        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-no-result.json", true)]
    public void ShouldAllowUseDifferentCredentials(JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        client.WithCredentials(new Uri("https://another.libsql.test")).ExecuteAsync("SELECT * FROM table");

        handler.LastSentRequest!.Uri.Should().Be("https://another.libsql.test/v3/pipeline");
    }

    [Theory]
    [JsonFileData("Data/execute-response-with-error.json", true)]
    public async Task ShouldThrowExceptionIfResultContainsError(
        string sql,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var action = () => client.QueryAsync(
            sql,
            TestDataJsonSerializerContext.Default.ResultSetTestModel);

        await action.Should().ThrowExactlyAsync<LibSqlClientExecutionException>();
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Fact]
    public async Task ShouldThrowExceptionIfStatusCodeIsNotSuccess()
    {
        var handler = new MockedJsonHttpHandler(new MockedJsonHttpResponse(HttpStatusCode.Forbidden, new { }));
        var client = new LibSqlHttpClient(handler);

        var action = () => client.ExecuteScalarAsync("SELECT * FROM table");

        await action.Should().ThrowExactlyAsync<LibSqlClientException>();
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-no-result.json", true)]
    public async Task ShouldThrowExceptionWhenQuerySingleWithoutResults(
        string sql,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var action = () => client.QuerySingleAsync(
            sql,
            TestDataJsonSerializerContext.Default.ResultSetTestModel);

        await action.Should().ThrowExactlyAsync<InvalidOperationException>();
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-no-result.json", true)]
    public async Task ShouldThrowExceptionWhenQueryFirstWithoutResults(
        string sql,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var action = () => client.QueryFirstAsync(
            sql,
            TestDataJsonSerializerContext.Default.ResultSetTestModel);

        await action.Should().ThrowExactlyAsync<InvalidOperationException>();
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-no-result.json", true)]
    public async Task ShouldReturnNullWhenQuerySingleOrDefaultWithoutResults(
        string sql,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var result = await client.QuerySingleOrDefaultAsync(
            sql,
            TestDataJsonSerializerContext.Default.ResultSetTestModel);

        result.Should().BeNull();
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-no-result.json", true)]
    public async Task ShouldReturnNullWhenQueryFirstOrDefaultWithoutResults(
        string sql,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var result = await client.QueryFirstOrDefaultAsync(
            sql,
            TestDataJsonSerializerContext.Default.ResultSetTestModel);

        result.Should().BeNull();
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-single-result.json", true)]
    public async Task ShouldReturnTheFirstItemWhenQuerySingleWithOneResult(
        string sql,
        ResultSetTestModel[] expected,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var result = await client.QuerySingleAsync(
            sql,
            TestDataJsonSerializerContext.Default.ResultSetTestModel);

        result.Should().BeEquivalentTo(expected[0]);
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-multiple-result.json", true)]
    public async Task ShouldThrowExceptionWhenQuerySingleWitMultipleResults(
        string sql,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var action = () => client.QuerySingleAsync(
            sql,
            TestDataJsonSerializerContext.Default.ResultSetTestModel);

        await action.Should().ThrowExactlyAsync<InvalidOperationException>();
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-multiple-result.json", true)]
    public async Task ShouldReturnExpectedResults(
        string sql,
        ResultSetTestModel[] expected,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var result = await client.QueryAsync(
            sql,
            TestDataJsonSerializerContext.Default.ResultSetTestModel);

        result.Should().BeEquivalentTo(expected);
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-no-result.json", true)]
    public async Task ShouldReturnNullIfNoResultsWhenExecuteScalar(
        string sql,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var result = await client.ExecuteScalarAsync(sql);

        result.Should().BeNull();
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-single-result.json", true)]
    public async Task ShouldReturnTheFirstValueOfTheFirstItemWhenExecuteScalar(
        string sql,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var result = await client.ExecuteScalarAsync(sql);

        result.Should().BeEquivalentTo(
            response.GetProperty("results")[0].GetProperty("response").GetProperty("result").GetProperty("rows")[0][0]
                .GetProperty("value").Deserialize<string>());
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Theory]
    [JsonFileData("Data/execute-response-no-error-single-result.json", true)]
    public async Task ShouldReturnAffectedRowsWhenExecute(
        string sql,
        JsonElement request,
        JsonElement response)
    {
        var (handler, client) = CreateClient(response);

        var result = await client.ExecuteAsync(sql);

        result.Should().Be(0);
        handler.LastSentRequest.Should().NotBeNull();
        handler.LastSentRequest!.Body.Should().BeEquivalentTo(JsonSerializer.Serialize(request));
    }

    [Theory]
    [JsonFileData("Data/batch-response-multiple-results.json", true)]
    public async Task ShouldParseMultipleResults(JsonElement response, ResultSetTestModel[][] expected)
    {
        var (_, client) = CreateClient(response);

        var result = await client.QueryMultipleAsync(
            ["SELECT * FROM table1", "SELECT * FROM TABLE 2"],
            TransactionMode.WriteImmediate);

        result.Count.Should().Be(expected.Length);

        for (var i = 0; i < expected.Length; i++)
        {
            result.HasMoreResults().Should().BeTrue();
            result.Read(TestDataJsonSerializerContext.Default.ResultSetTestModel).ToList().Should()
                .BeEquivalentTo(expected[i]);
        }

        result.HasMoreResults().Should().BeFalse();
    }

    private static (MockedJsonHttpHandler, LibSqlHttpClient) CreateClient(JsonElement response)
    {
        var handler = new MockedJsonHttpHandler(response);
        return (handler, new LibSqlHttpClient(handler, null, "token"));
    }
}
