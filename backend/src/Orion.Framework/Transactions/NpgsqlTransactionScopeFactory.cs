using Orion.Framework.Data;


namespace Orion.Framework.Transactions;

/// <summary>
/// Creates PostgreSQL transaction scopes using the existing
/// Orion database connection factory.
/// </summary>
public sealed class NpgsqlTransactionScopeFactory : ITransactionScopeFactory
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly TransactionContext _transactionContext;

    /// <summary>
    /// Initializes a new instance of the transaction scope factory.
    /// </summary>
    public NpgsqlTransactionScopeFactory(
        IConnectionFactory connectionFactory,
        TransactionContext transactionContext)
    {
        _connectionFactory = connectionFactory;
        _transactionContext = transactionContext;
    }

    /// <inheritdoc />
    public async Task<ITransactionScope> CreateAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            await _connectionFactory
                .CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

        var transaction = connection.BeginTransaction();

        _transactionContext.SetTransaction(
             connection,
             transaction);

        return new NpgsqlTransactionScope(
            connection,
            transaction,
            _transactionContext);
    }
}
