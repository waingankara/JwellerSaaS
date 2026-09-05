using Orion.Framework.Transactions;
using Xunit;

namespace JwellerSaaS.UnitTests.OrionFramework;

public sealed class TransactionalOperationExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_DelegatesOperationToTransactionManager()
    {
        var transactionManager = new FakeTransactionManager();
        var executor =
            new TransactionalOperationExecutor(
                transactionManager);

        var operation = new TestOperation();

        var result =
            await executor.ExecuteAsync(operation);

        Assert.Equal(42, result);
        Assert.True(transactionManager.WasCalled);
        Assert.NotNull(transactionManager.Operation);
    }

    private sealed class FakeTransactionManager : ITransactionManager
    {
        public bool WasCalled { get; private set; }

        public Delegate? Operation { get; private set; }

        public Task<TResult> ExecuteAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            Operation = operation;

            return operation(cancellationToken);
        }
    }

    private sealed class TestOperation
        : ITransactionalOperation<int>
    {
        public Task<int> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(42);
        }
    }
}
