namespace Orion.Framework.Transactions;

/// <summary>
/// Represents a business operation executed within the
/// Orion Transactional Framework.
/// </summary>
/// <typeparam name="TResult">
/// The result produced by the operation.
/// </typeparam>
public interface ITransactionalOperation<TResult>
{
    /// <summary>
    /// Executes the business operation.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The result produced by the business operation.
    /// </returns>
    Task<TResult> ExecuteAsync(
        CancellationToken cancellationToken = default);
}
