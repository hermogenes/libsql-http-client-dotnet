using System.Text.Json.Serialization;

namespace LibSql.Http.Client.Tests.Shared.Models;

public record ResultSetTestModel(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("salary")] float Salary,
    [property: JsonPropertyName("order")] int Order);