using System.Text.Json.Serialization;

namespace LibSql.Http.Client.Tests.Shared.Models;

[JsonSerializable(typeof(TestCaseStatement))]
[JsonSerializable(typeof(SerializationTestScenario))]
[JsonSerializable(typeof(HranaPipelineRequestBody))]
[JsonSerializable(typeof(ResultReaderTestScenario))]
[JsonSerializable(typeof(ResultSetTestModel))]
[JsonSourceGenerationOptions(
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class TestDataJsonSerializerContext : JsonSerializerContext;