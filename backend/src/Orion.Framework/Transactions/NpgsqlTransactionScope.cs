using System.Data;

namespace Orion.Framework.Transactions;

/// <summary>
/// Represents a PostgreSQL database transaction scope managed by
/// the Orion Framework.
/// </summary>
public sealed class NpgsqlTransactionScope : ITransactionScope
{
    private readonly IDbConnection _connection;
    private readonly System.Data.IDbTransaction _transaction;
    private readonly TransactionContext _transactionContext;

    private bool _completed;

    /// <summary>
    /// Initializes a new PostgreSQL transaction scope.
    /// </summary>
    public NpgsqlTransactionScope(
        IDbConnection connection,
        System.Data.IDbTransaction transaction,
        TransactionContext transactionContext)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(transactionContext);

        _connection = connection;
        _transaction = transaction;
        _transactionContext = transactionContext;
    }

    /// <inheritdoc />
    public Task CommitAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_completed)
        {
            return Task.CompletedTask;
        }

        _transaction.Commit();
        _completed = true;
        _transactionContext.Clear();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RollbackAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_completed)
        {
            return Task.CompletedTask;
        }

        _transaction.Rollback();
        _completed = true;
        _transactionContext.Clear();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (!_completed)
        {
            _transaction.Rollback();
            _transactionContext.Clear();
        }

        _transaction.Dispose();
        _connection.Dispose();

        return ValueTask.CompletedTask;
    }
}
