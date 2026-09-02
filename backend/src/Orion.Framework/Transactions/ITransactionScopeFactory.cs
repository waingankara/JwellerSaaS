namespace Orion.Framework.Transactions;

/// <summary>
/// Creates framework-managed database transaction scopes.
/// </summary>
public interface ITransactionScopeFactory
{
    /// <summary>
    /// Creates a new transaction scope.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel transaction creation.
    /// </param>
    /// <returns>
    /// A new transaction scope.
    /// </returns>
    Task<ITransactionScope> CreateAsync(
        CancellationToken cancellationToken = default);
}
