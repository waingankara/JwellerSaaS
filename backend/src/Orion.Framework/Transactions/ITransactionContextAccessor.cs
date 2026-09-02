namespace Orion.Framework.Transactions;

/// <summary>
/// Provides framework-internal control over the current transaction context.
/// </summary>
internal interface ITransactionContextAccessor
{
    /// <summary>
    /// Sets the current database transaction.
    /// </summary>
    void SetTransaction(
     System.Data.IDbConnection connection,
     System.Data.IDbTransaction transaction);

    /// <summary>
    /// Clears the current database transaction.
    /// </summary>
    void Clear();
}
