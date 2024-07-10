using System.Text.Json;
using System.Text.Json.Serialization;
using LibSql.Http.Client.Response;

namespace LibSql.Http.Client.Tests.Shared.Models;

public record ResultReaderTestScenario(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("response")]
    JsonDocument Response,
    [property: JsonPropertyName("expected")]
    ResultSetTestModel[]?[] Expected,
    [property: JsonPropertyName("commands")]
    bool[] Commands,
    [property: JsonPropertyName("errors")] List<ExecutionError> Errors,
    [property: JsonPropertyName("stats")] List<ExecutionStats> Stats)
{
    public override string ToString() => Name;
}