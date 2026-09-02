using System.Data;

namespace Orion.Framework.Data;

/// <summary>Executes parameterized SQL through Dapper.</summary>
public interface ISqlExecutor
{
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? parameters, CancellationToken cancellationToken);
    Task<T?> QueryFirstAsync<T>(string sql, object? parameters, CancellationToken cancellationToken);
    Task<int> ExecuteAsync(string sql, object? parameters, CancellationToken cancellationToken);
    Task<T?> ScalarAsync<T>(string sql, object? parameters, CancellationToken cancellationToken);
}
