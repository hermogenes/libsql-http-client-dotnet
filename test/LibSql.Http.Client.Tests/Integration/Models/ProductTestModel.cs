using System.Text.Json.Serialization;

namespace LibSql.Http.Client.Tests.Integration.Models;

public record ProductTestModel(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")]
    string Description,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("stock")] int Stock,
    [property: JsonPropertyName("image")] byte[]? Image);
