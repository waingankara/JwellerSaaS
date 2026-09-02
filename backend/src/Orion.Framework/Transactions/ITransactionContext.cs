using System.Data;

namespace Orion.Framework.Transactions;

/// <summary>
/// Provides access to the current database transaction.
/// </summary>
public interface ITransactionContext
{
    /// <summary>
    /// Gets the current database transaction.
    /// </summary>
    ///
    System.Data.IDbConnection? Connection { get; }
    System.Data.IDbTransaction? Transaction { get; }

    /// <summary>
    /// Gets a value indicating whether a transaction is currently active.
    /// </summary>
    bool IsActive { get; }
}
