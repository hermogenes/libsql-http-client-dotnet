using System.Text.Json;
using System.Text.Json.Serialization;
using LibSql.Http.Client.Request;

namespace LibSql.Http.Client.Tests.Shared.Models;

public record TestCaseStatementArg(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("value")] object? Value,
    [property: JsonPropertyName("hrana_value")] object? HranaValue);

public record TestCaseStatement(
    [property: JsonPropertyName("sql")] string Sql,
    [property: JsonPropertyName("args")] TestCaseStatementArg[]? Args = null,
    [property: JsonPropertyName("named_args")] TestCaseStatementArg[]? NamedArgs = null)
{
    public Statement ToStatement() => NamedArgs is not null
        ? new Statement(Sql, NamedArgs.ToDictionary(a => a.Name, a => ParseArgValue(a.Value, a.Type)))
        : new Statement(Sql, Args?.Select(a => ParseArgValue(a.Value, a.Type)).ToArray());

    private static object? ParseArgValue(object? argValue, string type)
    {
        if (argValue is not JsonElement jsonElement) return argValue;

        return type switch
        {
            "integer" => jsonElement.GetInt64(),
            "float" => jsonElement.GetDouble(),
            "text" => jsonElement.GetString(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}