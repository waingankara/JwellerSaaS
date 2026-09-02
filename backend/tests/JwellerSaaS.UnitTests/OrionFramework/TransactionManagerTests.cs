using Orion.Framework.Transactions;
using Xunit;

namespace JwellerSaaS.UnitTests.OrionFramework;

public sealed class TransactionManagerTests
{
    [Fact]
    public async Task ExecuteAsync_Commits_WhenOperationSucceeds()
    {
        var scope = new TestTransactionScope();
        var factory = new TestTransactionScopeFactory(scope);
        var manager = new TransactionManager(factory);

        var result = await manager.ExecuteAsync(
            _ => Task.FromResult(42));

        Assert.Equal(42, result);
        Assert.True(scope.CommitCalled);
        Assert.False(scope.RollbackCalled);
    }

    [Fact]
    public async Task ExecuteAsync_RollsBack_WhenOperationFails()
    {
        var scope = new TestTransactionScope();
        var factory = new TestTransactionScopeFactory(scope);
        var manager = new TransactionManager(factory);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.ExecuteAsync<int>(
                _ => throw new InvalidOperationException("Test failure")));

        Assert.False(scope.CommitCalled);
        Assert.True(scope.RollbackCalled);
    }

    private sealed class TestTransactionScopeFactory : ITransactionScopeFactory
    {
        private readonly TestTransactionScope _scope;

        public TestTransactionScopeFactory(TestTransactionScope scope)
        {
            _scope = scope;
        }

        public Task<ITransactionScope> CreateAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ITransactionScope>(_scope);
        }
    }

    private sealed class TestTransactionScope : ITransactionScope
    {
        public bool CommitCalled { get; private set; }

        public bool RollbackCalled { get; private set; }

        public Task CommitAsync(
            CancellationToken cancellationToken = default)
        {
            CommitCalled = true;

            return Task.CompletedTask;
        }

        public Task RollbackAsync(
            CancellationToken cancellationToken = default)
        {
            RollbackCalled = true;

            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
