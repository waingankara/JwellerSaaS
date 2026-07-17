namespace Orion.Framework.Transactions;

/// <summary>Coordinates database transactions for data access operations.</summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>Begins a transaction or nested savepoint where supported.</summary>
    Task BeginAsync(CancellationToken cancellationToken);
    /// <summary>Commits the active transaction.</summary>
    Task CommitAsync(CancellationToken cancellationToken);
    /// <summary>Rolls back the active transaction.</summary>
    Task RollbackAsync(CancellationToken cancellationToken);
}

/// <summary>Ambient transaction scope abstraction.</summary>
public interface IOrionTransactionScope : IAsyncDisposable
{
    /// <summary>Completes the scope.</summary>
    Task CommitAsync(CancellationToken cancellationToken);
    /// <summary>Abandons the scope.</summary>
    Task RollbackAsync(CancellationToken cancellationToken);
}
