using FluentAssertions;
using LibSql.Http.Client.Exceptions;
using LibSql.Http.Client.Request;
using LibSql.Http.Client.Tests.Integration.Fixture;

namespace LibSql.Http.Client.Tests.Integration;

public class RollbackTests() : TestWithContainersBase("products_rollback_scenarios")
{
    [Fact]
    public async Task CheckRollbackWhenOneCommandIsInvalid()
    {
        await InitializeContainer();

        var randomIndexSuccess = Random.Shared.Next(0, 5);
        var randomIndexFail = Random.Shared.Next(5, ProductTestData.Items.Count);

        var expectedItemSuccess = ProductTestData.Items[randomIndexSuccess];
        var expectedItemFail = ProductTestData.Items[randomIndexFail];

        Statement[] statements =
        [
            new Statement(
                TestData.InsertSqlWithPositionalArgs,
                [
                    expectedItemSuccess.Id, expectedItemSuccess.Name, expectedItemSuccess.Description,
                    expectedItemSuccess.Price, expectedItemSuccess.Stock, expectedItemSuccess.Image
                ]),
            new Statement(
                TestData.InsertSqlWithPositionalArgs,
                [
                    expectedItemFail.Id, expectedItemFail.Name, null, expectedItemFail.Price, expectedItemFail.Stock,
                    expectedItemFail.Image
                ])
        ];

        var beforeCount = await LibSqlClient.ExecuteScalarAsync(TestData.CountSql);

        var failingAction = () => LibSqlClient.ExecuteMultipleAsync(statements, TransactionMode.WriteImmediate);

        await failingAction.Should().ThrowExactlyAsync<LibSqlClientExecutionException>();

        var afterCount = await LibSqlClient.ExecuteScalarAsync(TestData.CountSql);

        afterCount.Should().Be(beforeCount);
    }
}
