using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Orion.Framework.Data;

/// <summary>PostgreSQL connection factory backed by Npgsql.</summary>
public sealed class NpgsqlConnectionFactory(IConfiguration configuration) : IConnectionFactory
{
    private const string ConnectionName = "DefaultConnection";

    /// <inheritdoc />
    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString(ConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{ConnectionName}' is not configured.");
        }

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
