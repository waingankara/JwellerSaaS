namespace Orion.Framework.Transactions;

/// <summary>
/// Executes transactional business operations within the
/// framework-managed transaction boundary.
/// </summary>
public interface ITransactionalOperationExecutor
{
    /// <summary>
    /// Executes the specified transactional operation.
    /// </summary>
    /// <typeparam name="TResult">
    /// The result produced by the operation.
    /// </typeparam>
    /// <param name="operation">
    /// The transactional business operation to execute.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The result produced by the operation.
    /// </returns>
    Task<TResult> ExecuteAsync<TResult>(
        ITransactionalOperation<TResult> operation,
        CancellationToken cancellationToken = default);
}
