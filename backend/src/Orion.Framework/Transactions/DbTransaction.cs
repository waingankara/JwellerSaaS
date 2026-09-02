using System.Data;

namespace Orion.Framework.Transactions;

/// <summary>
/// Wraps a database transaction and provides asynchronous
/// commit and rollback operations.
/// </summary>
public sealed class DbTransaction : IDbTransaction
{
    private bool _completed;

    /// <inheritdoc />
    public System.Data.IDbTransaction Transaction { get; }

    /// <summary>
    /// Initializes a new database transaction wrapper.
    /// </summary>
    /// <param name="transaction">
    /// The underlying database transaction.
    /// </param>
    public DbTransaction(System.Data.IDbTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        Transaction = transaction;
    }

    /// <inheritdoc />
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_completed)
        {
            return Task.CompletedTask;
        }

        Transaction.Commit();
        _completed = true;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_completed)
        {
            return Task.CompletedTask;
        }

        Transaction.Rollback();
        _completed = true;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Transaction.Dispose();

        return ValueTask.CompletedTask;
    }
}
