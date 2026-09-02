namespace Orion.Framework.Transactions;

/// <summary>
/// Executes business operations within a framework-managed transaction.
/// </summary>
public sealed class TransactionManager : ITransactionManager
{
    private readonly ITransactionScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the transaction manager.
    /// </summary>
    /// <param name="scopeFactory">
    /// Factory used to create transaction scopes.
    /// </param>
    public TransactionManager(ITransactionScopeFactory scopeFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);

        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var scope =
            await _scopeFactory
                .CreateAsync(cancellationToken)
                .ConfigureAwait(false);

        try
        {
            var result = await operation(cancellationToken)
                .ConfigureAwait(false);

            await scope
                .CommitAsync(cancellationToken)
                .ConfigureAwait(false);

            return result;
        }
        catch
        {
            await scope
                .RollbackAsync(cancellationToken)
                .ConfigureAwait(false);

            throw;
        }
    }
}
