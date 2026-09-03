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

        var sqlExecutor =
            new SqlExecutor(
                connectionFactory,
                transactionContext);

        await transactionManager.ExecuteAsync(
            async cancellationToken =>
            {
                await sqlExecutor.ExecuteAsync(
                    """
                    INSERT INTO transaction_test (value)
                    VALUES ('commit-test');
                    """,
                    null,
                    cancellationToken);

                return true;
            });

        Assert.False(transactionContext.IsActive);

        var verificationContext =
            new TransactionContext();

        var verificationExecutor =
            new SqlExecutor(
                connectionFactory,
                verificationContext);

        var count = await verificationExecutor.ScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM transaction_test
            WHERE value = 'commit-test';
            """,
            null,
            CancellationToken.None);

        Assert.True(count >= 1);
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

        var sqlExecutor =
            new SqlExecutor(
                connectionFactory,
                transactionContext);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => transactionManager.ExecuteAsync<bool>(
                async cancellationToken =>
                {
                    await sqlExecutor.ExecuteAsync(
                        """
                        INSERT INTO transaction_test (value)
                        VALUES ('rollback-test');
                        """,
                        null,
                        cancellationToken);

                    throw new InvalidOperationException("Rollback test");
                }));

        Assert.False(transactionContext.IsActive);

        var verificationContext =
            new TransactionContext();

        var verificationExecutor =
            new SqlExecutor(
                connectionFactory,
                verificationContext);

        var count = await verificationExecutor.ScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM transaction_test
            WHERE value = 'rollback-test';
            """,
            null,
            CancellationToken.None);

        Assert.Equal(0, count);
    }
}
