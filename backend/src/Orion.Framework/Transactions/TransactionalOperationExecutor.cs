namespace Orion.Framework.Transactions;

/// <summary>
/// Executes transactional business operations within the
/// framework-managed transaction boundary.
/// </summary>
public sealed class TransactionalOperationExecutor(
    ITransactionManager transactionManager)
    : ITransactionalOperationExecutor
{
    /// <inheritdoc />
    public Task<TResult> ExecuteAsync<TResult>(
        ITransactionalOperation<TResult> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return transactionManager.ExecuteAsync(
            operation.ExecuteAsync,
            cancellationToken);
    }
}
