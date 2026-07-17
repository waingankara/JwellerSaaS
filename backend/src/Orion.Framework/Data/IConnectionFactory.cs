using System.Data;

namespace Orion.Framework.Data;

/// <summary>Creates database connections for application data access.</summary>
public interface IConnectionFactory
{
    /// <summary>Creates and opens a database connection.</summary>
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
