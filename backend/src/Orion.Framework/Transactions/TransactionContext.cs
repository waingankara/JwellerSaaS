using System.Data;

namespace Orion.Framework.Transactions;

/// <summary>
/// Stores the database transaction associated with the current execution scope.
/// </summary>
public sealed class TransactionContext : ITransactionContext, ITransactionContextAccessor
{
    /// <inheritdoc />
    ///
    public System.Data.IDbConnection? Connection { get; private set; }
    public System.Data.IDbTransaction? Transaction { get; private set; }

    /// <inheritdoc />
    public bool IsActive => Transaction is not null;

    /// <summary>
    /// Sets the current database transaction.
    /// </summary>
    /// /// <param name="connection">
    /// The database connection associated with the transaction.
    /// </param>
    /// <param name="transaction">
    /// The database transaction to associate with the current context.
    /// </param>
    public void SetTransaction(
     System.Data.IDbConnection connection,
     System.Data.IDbTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        Connection = connection;
        Transaction = transaction;
    }

    /// <summary>
    /// Clears the current database transaction.
    /// </summary>
    public void Clear()
    {
        Transaction = null;
        Connection = null;
    }
}
