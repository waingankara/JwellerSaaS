using System.Data;
using Dapper;

namespace Orion.Framework.Data;

/// <summary>Dapper-backed SQL executor.</summary>
public sealed class SqlExecutor(IConnectionFactory connectionFactory) : ISqlExecutor
{
    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? parameters, CancellationToken cancellationToken) { using var c=await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false); var r=await c.QueryAsync<T>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)).ConfigureAwait(false); return r.AsList(); }
    public async Task<T?> QueryFirstAsync<T>(string sql, object? parameters, CancellationToken cancellationToken) { using var c=await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false); return await c.QueryFirstOrDefaultAsync<T>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)).ConfigureAwait(false); }
    public async Task<int> ExecuteAsync(string sql, object? parameters, CancellationToken cancellationToken) { using var c=await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false); return await c.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)).ConfigureAwait(false); }
    public async Task<T?> ScalarAsync<T>(string sql, object? parameters, CancellationToken cancellationToken) { using var c=await connectionFactory.CreateOpenConnectionAsync(cancellationToken).ConfigureAwait(false); return await c.ExecuteScalarAsync<T>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)).ConfigureAwait(false); }
}
