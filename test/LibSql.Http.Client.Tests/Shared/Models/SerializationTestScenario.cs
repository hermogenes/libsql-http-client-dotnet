using System.Text.Json.Serialization;
using LibSql.Http.Client.Request;

namespace LibSql.Http.Client.Tests.Shared.Models;

public record SerializationTestScenario(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("statements")]
    TestCaseStatement[] Statements,
    [property: JsonPropertyName("transaction")]
    TransactionMode Transaction,
    [property: JsonPropertyName("request")]
    HranaPipelineRequestBody Request,
    [property: JsonPropertyName("baton")] string? Baton = null,
    [property: JsonPropertyName("is_interactive")]
    bool IsInteractive = false)
{
    public override string ToString() => Name;
}