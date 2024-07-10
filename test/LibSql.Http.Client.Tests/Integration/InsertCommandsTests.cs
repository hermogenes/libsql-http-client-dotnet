using FluentAssertions;
using LibSql.Http.Client.Request;
using LibSql.Http.Client.Tests.Integration.Fixture;
using LibSql.Http.Client.Tests.Integration.Models;

namespace LibSql.Http.Client.Tests.Integration;

public class InsertCommandsTests() : TestWithContainersBase("products_insert_commands_scenarios")
{
    [Fact]
    public async Task CheckInsertCommandsUsingPositionalArgs()
    {
        await InitializeContainer();

        var items = ProductTestData.Items.Where(i => i.Image is null).ToArray();

        var statements = items.Select(
            i => new Statement(
                TestData.InsertSqlWithPositionalArgs,
                [i.Id, i.Name, i.Description, i.Price, i.Stock, i.Image])).ToArray();

        var results = await LibSqlClient.ExecuteMultipleAsync(statements, TransactionMode.WriteImmediate);

        results.Should().Be(statements.Length);

        var insertedItems = await LibSqlClient.QueryAsync<ProductTestModel>(
            TestData.SelectProductsWithoutImageSql,
            IntegrationTestsSerializerContext.Default.ProductTestModel);

        insertedItems.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task CheckInsertCommandsUsingNamedArgs()
    {
        await InitializeContainer();

        var items = ProductTestData.Items.Where(i => i.Image is not null).ToArray();

        var statements = items.Select(
            i => new Statement(
                TestData.InsertSqlWithNamedArgs,
                new Dictionary<string, object?>
                {
                    { "id", i.Id },
                    { "name", i.Name },
                    { "description", i.Description },
                    { "price", i.Price },
                    { "stock", i.Stock },
                    { "image", i.Image }
                })).ToArray();

        var results = await LibSqlClient.ExecuteMultipleAsync(statements, TransactionMode.WriteImmediate);

        results.Should().Be(statements.Length);

        var insertedItems = await LibSqlClient.QueryAsync<ProductTestModel>(
            TestData.SelectProductsWithImageSql,
            IntegrationTestsSerializerContext.Default.ProductTestModel);

        insertedItems.Should().BeEquivalentTo(items);
    }
}
