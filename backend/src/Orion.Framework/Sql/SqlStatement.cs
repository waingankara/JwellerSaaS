namespace Orion.Framework.Sql;

/// <summary>Represents parameterized SQL and its parameter object.</summary>
public sealed record SqlStatement(string Sql, object? Parameters);
