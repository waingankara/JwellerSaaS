using Microsoft.Extensions.Configuration;
using Orion.Framework.Data;
using Orion.Framework.Transactions;
using Xunit;

namespace JwellerSaaS.IntegrationTests;

public sealed class TransactionIntegrationTests
{
    [Fact]
    public async Task TransactionManager_CommitsDatabaseChanges()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(
                "appsettings.json",
                optional: false)
            .Build();

        var connectionFactory =
            new NpgsqlConnectionFactory(configuration);

        var transactionContext =
            new TransactionContext();

        var scopeFactory =
            new NpgsqlTransactionScopeFactory(
                connectionFactory,
                transactionContext);

        var transactionManager =
            new TransactionManager(scopeFactory);

        await transactionManager.ExecuteAsync(
                 cancellationToken =>
                 {
                     using var command =
                         transactionContext.Connection!.CreateCommand();

                     command.Transaction =
                         transactionContext.Transaction;

                     command.CommandText =
                         "INSERT INTO transaction_test (value) VALUES ('commit-test');";

                     command.ExecuteNonQuery();

                     return Task.FromResult(true);
                 });

        Assert.False(transactionContext.IsActive);
    }

    [Fact]
    public async Task TransactionManager_RollsBackDatabaseChanges()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(
                "appsettings.json",
                optional: false)
            .Build();

        var connectionFactory =
            new NpgsqlConnectionFactory(configuration);

        var transactionContext =
            new TransactionContext();

        var scopeFactory =
            new NpgsqlTransactionScopeFactory(
                connectionFactory,
                transactionContext);

        var transactionManager =
            new TransactionManager(scopeFactory);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => transactionManager.ExecuteAsync(
                cancellationToken =>
                {
                    using var command =
                        transactionContext.Connection!.CreateCommand();

                    command.Transaction =
                        transactionContext.Transaction;

                    command.CommandText =
                        "INSERT INTO transaction_test (value) VALUES ('rollback-test');";

                    command.ExecuteNonQuery();

                    throw new InvalidOperationException("Rollback test");

#pragma warning disable CS0162
                    return Task.FromResult(true);
#pragma warning restore CS0162
                }));

        Assert.False(transactionContext.IsActive);
    }
}
