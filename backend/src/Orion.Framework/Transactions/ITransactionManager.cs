namespace Orion.Framework.Transactions;

/// <summary>
/// Defines the framework boundary for executing business operations
/// within a database transaction.
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Executes the specified operation within a database transaction.
    /// </summary>
    /// <typeparam name="TResult">
    /// The result returned by the operation.
    /// </typeparam>
    /// <param name="operation">
    /// The business operation to execute.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The result produced by the business operation.
    /// </returns>
    Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
