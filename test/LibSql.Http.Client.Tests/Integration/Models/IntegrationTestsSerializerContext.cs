using System.Text.Json.Serialization;

namespace LibSql.Http.Client.Tests.Integration.Models;

[JsonSerializable(typeof(ProductTestModel))]
public partial class IntegrationTestsSerializerContext : JsonSerializerContext
{
}
