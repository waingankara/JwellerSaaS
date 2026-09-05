using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orion.Framework.Crud;
using Orion.Framework.Data;
using Orion.Framework.DependencyInjection;
using Orion.Framework.Security;
using Orion.Framework.Tenancy;
using Orion.Framework.Transactions;
using Xunit;

namespace JwellerSaaS.IntegrationTests;

public sealed class TransactionIntegrationTests
{
    [Fact]
    public async Task TransactionalOperationExecutor_CommitsDatabaseChanges()
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

        var transactionalExecutor =
            new TransactionalOperationExecutor(
                transactionManager);

        var sqlExecutor =
            new SqlExecutor(
                connectionFactory,
                transactionContext);

        var operation =
            new CommitTestOperation(sqlExecutor);

        var result =
            await transactionalExecutor.ExecuteAsync(operation);

        Assert.True(result);
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
    public async Task TransactionalOperationExecutor_RollsBackDatabaseChanges()
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

        var transactionalExecutor =
            new TransactionalOperationExecutor(
                transactionManager);

        var sqlExecutor =
            new SqlExecutor(
                connectionFactory,
                transactionContext);

        var operation =
            new RollbackTestOperation(sqlExecutor);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => transactionalExecutor.ExecuteAsync(operation));

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

    [Fact]
    public async Task GenericCrud_CreateAsync_PopulatesGeneratedId()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(
                "appsettings.json",
                optional: false)
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddOrionFramework();

        services.AddScoped<
            ITenantContextAccessor,
            TestTenantContextAccessor>();

        services.AddScoped<
            ICurrentUserAccessor,
            TestCurrentUserAccessor>();

        await using var provider =
            services.BuildServiceProvider();

        using var scope =
            provider.CreateScope();

        var crudService =
            scope.ServiceProvider
                .GetRequiredService<
                    ICrudService<FrameworkIntegrationTest>>();

        var transactionExecutor =
            scope.ServiceProvider
                .GetRequiredService<
                    ITransactionalOperationExecutor>();

        var entity =
            new FrameworkIntegrationTest
            {
                Value =
                    $"generated-key-{Guid.NewGuid():N}"
            };

        var operation =
            new CreateFrameworkIntegrationTestOperation(
                crudService,
                entity);

        var result =
            await transactionExecutor.ExecuteAsync(
                operation);

        Assert.True(result);
        Assert.True(
            entity.FrameworkIntegrationTestId > 0);
    }

    private sealed class CommitTestOperation(
        ISqlExecutor sqlExecutor)
        : ITransactionalOperation<bool>
    {
        public async Task<bool> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            await sqlExecutor.ExecuteAsync(
                """
                INSERT INTO transaction_test (value)
                VALUES ('commit-test');
                """,
                null,
                cancellationToken);

            return true;
        }
    }

    private sealed class RollbackTestOperation(
        ISqlExecutor sqlExecutor)
        : ITransactionalOperation<bool>
    {
        public async Task<bool> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            await sqlExecutor.ExecuteAsync(
                """
                INSERT INTO transaction_test (value)
                VALUES ('rollback-test');
                """,
                null,
                cancellationToken);

            throw new InvalidOperationException(
                "Rollback test");
        }
    }

    private sealed class CreateFrameworkIntegrationTestOperation(
        ICrudService<FrameworkIntegrationTest> crudService,
        FrameworkIntegrationTest entity)
        : ITransactionalOperation<bool>
    {
        public async Task<bool> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            await crudService.CreateAsync(
                entity,
                cancellationToken);

            return true;
        }
    }

    private sealed class TestTenantContextAccessor
        : ITenantContextAccessor
    {
        public TenantContext TenantContext { get; } =
            new(1, false);
    }

    private sealed class TestCurrentUserAccessor
        : ICurrentUserAccessor
    {
        public CurrentUser CurrentUser { get; } =
            new(
                1,
                "integration-test",
                "integration@test.local",
                1,
                null,
                new HashSet<string>(),
                new HashSet<string>(),
                null,
                null);
    }
}
