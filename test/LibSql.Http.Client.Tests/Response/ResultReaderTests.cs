using System.Text.Json;
using FluentAssertions;
using LibSql.Http.Client.Response;
using LibSql.Http.Client.Tests.Shared.Attributes;
using LibSql.Http.Client.Tests.Shared.Models;

namespace LibSql.Http.Client.Tests.Response;

public class ResultReaderTests
{
    [Theory]
    [JsonFileData("Data/execute-response.json")]
    public async Task ShouldParseResponse(ResultReaderTestScenario scenario)
    {
        using var stream = new MemoryStream();
        await using var writer = new Utf8JsonWriter(stream);
        scenario.Response.RootElement.WriteTo(writer);
        await writer.FlushAsync();
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = await ResultReader.ParseAsync(stream);

        reader.Count.Should().Be(scenario.Expected.Length);

        foreach (var t in scenario.Expected)
        {
            reader.HasMoreResults().Should().BeTrue();
            var items = reader.Read(TestDataJsonSerializerContext.Default.ResultSetTestModel);
            items.Should().BeEquivalentTo(t);
        }

        reader.Stats.Should().BeEquivalentTo(scenario.Stats);
        reader.AffectedRows.Should().Be(scenario.Stats.Sum(s => s.AffectedRows));

        reader.HasMoreResults().Should().BeFalse();
        reader.Errors.Should().BeEquivalentTo(scenario.Errors);
        reader.Baton.Should().Be(null);
    }

    [Theory]
    [JsonFileData("Data/batch-response.json")]
    public async Task ShouldParseBatchResponse(ResultReaderTestScenario scenario)
    {
        using var stream = new MemoryStream();
        await using var writer = new Utf8JsonWriter(stream);
        scenario.Response.RootElement.WriteTo(writer);
        await writer.FlushAsync();
        stream.Seek(0, SeekOrigin.Begin);
        var reader = await ResultReader.ParseAsync(stream);

        reader.Count.Should().Be(scenario.Expected.Length);

        foreach (var t in scenario.Expected)
        {
            reader.HasMoreResults().Should().BeTrue();
            var items = reader.Read(TestDataJsonSerializerContext.Default.ResultSetTestModel);
            items.Should().BeEquivalentTo(t);
        }

        reader.Errors.Should().BeEquivalentTo(scenario.Errors);
        reader.Baton.Should().Be(null);
    }

    [Theory]
    [JsonFileData("Data/execute-response.json")]
    public async Task TryReadMoreResultsThanExistsThrowError(ResultReaderTestScenario scenario)
    {
        using var stream = new MemoryStream();
        await using var writer = new Utf8JsonWriter(stream);
        scenario.Response.RootElement.WriteTo(writer);
        await writer.FlushAsync();
        stream.Seek(0, SeekOrigin.Begin);
        var reader = await ResultReader.ParseAsync(stream);

        var action = () =>
            reader.ReadAt(reader.Count, TestDataJsonSerializerContext.Default.ResultSetTestModel);
        var action2 = () =>
            reader.ReadAt((reader.Count + 1) * -1, TestDataJsonSerializerContext.Default.ResultSetTestModel);

        reader.Count.Should().Be(scenario.Expected.Length);
        action.Should().ThrowExactly<IndexOutOfRangeException>();
        action2.Should().ThrowExactly<IndexOutOfRangeException>();
    }
}
