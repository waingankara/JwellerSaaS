using System.Data;
using Dapper;
using Orion.Framework.Transactions;

namespace Orion.Framework.Data;

/// <summary>
/// Dapper-backed SQL executor.
/// </summary>
public sealed class SqlExecutor(
    IConnectionFactory connectionFactory,
    ITransactionContext transactionContext) : ISqlExecutor
{
    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters,
        CancellationToken cancellationToken)
    {
        if (transactionContext.IsActive)
        {
            var command = new CommandDefinition(
                sql,
                parameters,
                transactionContext.Transaction,
                cancellationToken: cancellationToken);

            var result = await transactionContext.Connection!
                .QueryAsync<T>(command)
                .ConfigureAwait(false);

            return result.AsList();
        }

        using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

        var standaloneCommand = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var standaloneResult = await connection
            .QueryAsync<T>(standaloneCommand)
            .ConfigureAwait(false);

        return standaloneResult.AsList();
    }

    public async Task<T?> QueryFirstAsync<T>(
        string sql,
        object? parameters,
        CancellationToken cancellationToken)
    {
        if (transactionContext.IsActive)
        {
            var command = new CommandDefinition(
                sql,
                parameters,
                transactionContext.Transaction,
                cancellationToken: cancellationToken);

            return await transactionContext.Connection!
                .QueryFirstOrDefaultAsync<T>(command)
                .ConfigureAwait(false);
        }

        using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

        var standaloneCommand = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        return await connection
            .QueryFirstOrDefaultAsync<T>(standaloneCommand)
            .ConfigureAwait(false);
    }

    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters,
        CancellationToken cancellationToken)
    {
        if (transactionContext.IsActive)
        {
            var command = new CommandDefinition(
                sql,
                parameters,
                transactionContext.Transaction,
                cancellationToken: cancellationToken);

            return await transactionContext.Connection!
                .ExecuteAsync(command)
                .ConfigureAwait(false);
        }

        using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

        var standaloneCommand = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        return await connection
            .ExecuteAsync(standaloneCommand)
            .ConfigureAwait(false);
    }

    public async Task<T?> ScalarAsync<T>(
        string sql,
        object? parameters,
        CancellationToken cancellationToken)
    {
        if (transactionContext.IsActive)
        {
            var command = new CommandDefinition(
                sql,
                parameters,
                transactionContext.Transaction,
                cancellationToken: cancellationToken);

            return await transactionContext.Connection!
                .ExecuteScalarAsync<T>(command)
                .ConfigureAwait(false);
        }

        using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

        var standaloneCommand = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        return await connection
            .ExecuteScalarAsync<T>(standaloneCommand)
            .ConfigureAwait(false);
    }
}
