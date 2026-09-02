using System.Data;

namespace Orion.Framework.Transactions;

/// <summary>
/// Represents a database transaction owned by the Orion Framework.
/// </summary>
public interface IDbTransaction
{
    /// <summary>
    /// Gets the underlying database transaction.
    /// </summary>
    System.Data.IDbTransaction Transaction { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
