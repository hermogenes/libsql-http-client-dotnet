using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using FluentAssertions;
using LibSql.Http.Client.Request;
using LibSql.Http.Client.Tests.Shared.Attributes;
using LibSql.Http.Client.Tests.Shared.Models;

namespace LibSql.Http.Client.Tests.Request;

public class RequestSerializerTests
{
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new(TestDataJsonSerializerContext.Default.HranaPipelineRequestBody.Options)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

    [Theory]
    [JsonFileData("Data/execute.json")]
    public async Task CheckSingleExecuteSerialization(SerializationTestScenario scenario)
    {
        var statement = scenario.Statements[0].ToStatement();

        var output = await RequestSerializer.Serialize(
            [statement],
            scenario.Transaction,
            scenario.Baton,
            scenario.IsInteractive);

        var strOutput = await output.ReadAsStringAsync();

        var requestBody = JsonSerializer.Serialize(scenario.Request, _jsonSerializerOptions);

        strOutput.Should().Be(requestBody);
    }

    [Theory]
    [JsonFileData("Data/batch.json")]
    public async Task CheckBatchSerialization(SerializationTestScenario scenario)
    {
        var output = await RequestSerializer.Serialize(
            scenario.Statements.Select(s => s.ToStatement()).ToArray(),
            scenario.Transaction,
            scenario.Baton,
            scenario.IsInteractive);

        var strOutput = await output.ReadAsStringAsync();

        var requestBody = JsonSerializer.Serialize(scenario.Request, _jsonSerializerOptions);

        strOutput.Should().Be(requestBody);
    }

    [Fact]
    public void ShouldThrowExceptionIfArgValueIsNotSupported()
    {
        var action = () =>
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            RequestSerializer.WriteArgObject(writer, new { type = "unsupported" });
        };

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData((float)1)]
    [InlineData((double)1)]
    public void ShouldParseFloatLikeValue(object value)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        RequestSerializer.WriteArgObject(writer, value);

        writer.Flush();

        var strJson = Encoding.UTF8.GetString(stream.ToArray());

        strJson.Should().Be($"{{\"type\":\"float\",\"value\":{value}}}");
    }

    [Fact]
    public void ShouldParseDecimalAsFloatValue()
    {
        const decimal value = 1;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        RequestSerializer.WriteArgObject(writer, value);

        writer.Flush();

        var strJson = Encoding.UTF8.GetString(stream.ToArray());

        strJson.Should().Be($"{{\"type\":\"float\",\"value\":{value}}}");
    }

    [Fact]
    public void ShouldParseBlobValue()
    {
        byte[] value = [1, 2, 3];
        const string expected = "AQID";
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        RequestSerializer.WriteArgObject(writer, value);

        writer.Flush();

        var strJson = Encoding.UTF8.GetString(stream.ToArray());

        strJson.Should().Be($"{{\"type\":\"blob\",\"base64\":\"{expected}\"}}");
    }

    [Fact]
    public void ShouldParseNullValue()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        RequestSerializer.WriteArgObject(writer, null);

        writer.Flush();

        var strJson = Encoding.UTF8.GetString(stream.ToArray());

        strJson.Should().Be("{\"type\":\"null\"}");
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData((long)1, 1)]
    [InlineData((short)1, 1)]
    [InlineData((ulong)1, 1)]
    [InlineData((uint)1, 1)]
    [InlineData((ushort)1, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void ShouldParseIntegerLikeValue(object value, object expected)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        RequestSerializer.WriteArgObject(writer, value);

        writer.Flush();

        var strJson = Encoding.UTF8.GetString(stream.ToArray());

        strJson.Should().Be($"{{\"type\":\"integer\",\"value\":\"{expected}\"}}");
    }
}
