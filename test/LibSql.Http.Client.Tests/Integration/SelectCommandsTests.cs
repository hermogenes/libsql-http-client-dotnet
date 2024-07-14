using FluentAssertions;
using LibSql.Http.Client.Request;
using LibSql.Http.Client.Tests.Integration.Fixture;
using LibSql.Http.Client.Tests.Integration.Models;

namespace LibSql.Http.Client.Tests.Integration;

public class SelectCommandsTests() : TestWithContainersBase("products_select_commands_scenarios")
{
    [Fact]
    public async Task CheckHealth()
    {
        await InitializeContainer();

        var isHealth = await LibSqlClient.HealthCheckAsync();

        isHealth.Should().BeTrue();
    }
    
    [Fact]
    public async Task CheckSelectAll()
    {
        await InitializeContainer();

        var allItems = await LibSqlClient.QueryAsync(
            TestData.SelectAllSql,
            IntegrationTestsSerializerContext.Default.ProductTestModel);

        allItems.Should().BeEquivalentTo(ProductTestData.Items);
    }

    [Fact]
    public async Task CheckSelectById()
    {
        await InitializeContainer();

        var randomIndex = Random.Shared.Next(0, ProductTestData.Items.Count);

        var expectedItem = ProductTestData.Items[randomIndex];

        var item = await LibSqlClient.QuerySingleAsync(
            new Statement(TestData.SelectByIdSql, [expectedItem.Id]),
            IntegrationTestsSerializerContext.Default.ProductTestModel);

        item.Should().BeEquivalentTo(expectedItem);
    }

    [Fact]
    public async Task CheckSelectByNameLike()
    {
        await InitializeContainer();

        var expectedItem = ProductTestData.Items[0];

        var item = await LibSqlClient.QueryFirstAsync(
            new Statement(TestData.SelectLikeNameSql, [$"%{expectedItem.Name}%"]),
            IntegrationTestsSerializerContext.Default.ProductTestModel);

        item.Should().BeEquivalentTo(expectedItem);
    }

    protected override async Task InitializeContainer()
    {
        await base.InitializeContainer();

        var statements = ProductTestData.Items.Select(
            i => new Statement(
                TestData.InsertSqlWithPositionalArgs,
                [i.Id, i.Name, i.Description, i.Price, i.Stock, i.Image])).ToArray();

        await LibSqlClient.ExecuteMultipleAsync(statements, TransactionMode.WriteImmediate);
    }
}
