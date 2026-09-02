using Dapper;
using Orion.Framework.Metadata;
using Orion.Framework.Pagination;
using Orion.Framework.Query;
using Orion.Framework.Search;

using FilterDefinition = Orion.Framework.Query.FilterDefinition;

namespace Orion.Framework.Sql;

/// <summary>
/// Builds parameterized INSERT statements.
/// </summary>
public sealed class InsertBuilder
{
    /// <summary>
    /// Builds an INSERT statement using the supplied master metadata.
    /// </summary>
    public SqlStatement Build(
        MasterDefinition definition,
        object values)
    {
        var columns = definition.Columns
            .Where(column => !column.IsPrimaryKey)
            .ToArray();

        var columnNames = string.Join(
            ", ",
            columns.Select(column =>
                SqlName.Identifier(column.ColumnName)));

        var parameterNames = string.Join(
            ", ",
            columns.Select(column =>
                $"@{column.PropertyName}"));

        var sql =
            $"INSERT INTO {SqlName.Identifier(definition.TableName)} " +
            $"({columnNames}) " +
            $"VALUES ({parameterNames})";

        return new SqlStatement(sql, values);
    }
}

/// <summary>
/// Builds parameterized UPDATE statements.
/// </summary>
public sealed class UpdateBuilder
{
    /// <summary>
    /// Builds an UPDATE statement using the supplied master metadata.
    /// </summary>
    public SqlStatement Build(
        MasterDefinition definition,
        object values)
    {
        var key = definition.Columns
            .First(column => column.IsPrimaryKey);

        var columns = definition.Columns
            .Where(column => !column.IsPrimaryKey)
            .ToArray();

        var assignments = string.Join(
            ", ",
            columns.Select(column =>
                $"{SqlName.Identifier(column.ColumnName)} = @{column.PropertyName}"));

        var tenantPredicate = definition.Tenant.TenantId is null
            ? string.Empty
            : $" AND {SqlName.Identifier(definition.Tenant.TenantId.ColumnName)} " +
              $"= @{definition.Tenant.TenantId.PropertyName}";

        var sql =
            $"UPDATE {SqlName.Identifier(definition.TableName)} " +
            $"SET {assignments} " +
            $"WHERE {SqlName.Identifier(key.ColumnName)} = @{key.PropertyName}" +
            tenantPredicate;

        return new SqlStatement(sql, values);
    }
}

/// <summary>
/// Builds parameterized DELETE statements.
/// </summary>
public sealed class DeleteBuilder
{
    /// <summary>
    /// Builds a DELETE statement using the supplied master metadata.
    /// </summary>
    public SqlStatement Build(
        MasterDefinition definition,
        object parameters)
    {
        var key = definition.Columns
            .First(column => column.IsPrimaryKey);

        var tenantPredicate = definition.Tenant.TenantId is null
            ? string.Empty
            : $" AND {SqlName.Identifier(definition.Tenant.TenantId.ColumnName)} = @TenantId";

        var sql =
            $"DELETE FROM {SqlName.Identifier(definition.TableName)} " +
            $"WHERE {SqlName.Identifier(key.ColumnName)} = @{key.PropertyName}" +
            tenantPredicate;

        return new SqlStatement(sql, parameters);
    }
}

/// <summary>
/// Builds parameterized SELECT statements.
/// </summary>
public sealed class SelectBuilder
{
    /// <summary>
    /// Builds a SELECT statement using the supplied master metadata.
    /// </summary>
    public SqlStatement Build(
        MasterDefinition definition,
        object? parameters = null)
    {
        var columns = string.Join(
            ", ",
            definition.Columns.Select(column =>
                $"{SqlName.Identifier(column.ColumnName)} AS \"{column.PropertyName}\""));

        var tableName = SqlName.Identifier(definition.TableName);

        var sql = $"SELECT {columns} FROM {tableName}";

        return new SqlStatement(sql, parameters);
    }
}

/// <summary>
/// Builds parameterized EXISTS statements.
/// </summary>
public sealed class ExistsBuilder
{
    /// <summary>
    /// Builds an EXISTS statement using the supplied master metadata.
    /// </summary>
    public SqlStatement Build(MasterDefinition definition)
    {
        var key = definition.Columns
            .First(column => column.IsPrimaryKey);

        var sql =
            $"SELECT EXISTS (" +
            $"SELECT 1 FROM {SqlName.Identifier(definition.TableName)} " +
            $"WHERE {SqlName.Identifier(key.ColumnName)} = @{key.PropertyName})";

        return new SqlStatement(sql, null);
    }
}

/// <summary>
/// Builds parameterized COUNT statements.
/// </summary>
public sealed class CountBuilder
{
    /// <summary>
    /// Builds a COUNT statement using the supplied master metadata.
    /// </summary>
    public SqlStatement Build(MasterDefinition definition)
    {
        var sql =
            $"SELECT COUNT(1) " +
            $"FROM {SqlName.Identifier(definition.TableName)}";

        return new SqlStatement(sql, null);
    }
}

/// <summary>
/// Builds duplicate-check statements.
/// </summary>
public sealed class DuplicateBuilder
{
    /// <summary>
    /// Builds a duplicate-check statement using the supplied master metadata.
    /// </summary>
    public SqlStatement Build(
        MasterDefinition definition,
        object values)
    {
        var columns = definition.Duplicate.Columns;

        if (columns.Count == 0)
        {
            throw new InvalidOperationException(
                "Duplicate check requires duplicate columns.");
        }

        var predicates = string.Join(
            " AND ",
            columns.Select(column =>
                $"{SqlName.Identifier(column.ColumnName)} = @{column.PropertyName}"));

        var sql =
            $"SELECT EXISTS (" +
            $"SELECT 1 FROM {SqlName.Identifier(definition.TableName)} " +
            $"WHERE {predicates})";

        return new SqlStatement(sql, values);
    }
}

/// <summary>
/// Builds pagination clauses.
/// </summary>
public sealed class PaginationBuilder
{
    /// <summary>
    /// Builds LIMIT and OFFSET clauses.
    /// </summary>
    public string Build(PagedRequest request)
    {
        return "LIMIT @PageSize OFFSET @Offset";
    }
}

/// <summary>
/// Builds parameterized search predicates.
/// </summary>
public sealed class SearchBuilder
{
    /// <summary>
    /// Builds a WHERE clause from filters.
    /// </summary>
    public SqlStatement Build(
        MasterDefinition definition,
        IReadOnlyList<FilterDefinition> filters)
    {
        var parameters = new DynamicParameters();
        var parts = new List<string>();

        var parameterIndex = 0;

        foreach (var filter in filters)
        {
            var column = definition.Columns.First(
                candidate =>
                    string.Equals(
                        candidate.PropertyName,
                        filter.Field,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        candidate.ColumnName,
                        filter.Field,
                        StringComparison.OrdinalIgnoreCase));

            var parameterName = $"p{parameterIndex++}";

            var sql = ToSql(
                column,
                filter,
                parameterName,
                parameters);

            parts.Add(sql);
        }

        var whereClause = parts.Count == 0
            ? string.Empty
            : $"WHERE {string.Join(" AND ", parts)}";

        return new SqlStatement(
            whereClause,
            parameters);
    }

    private static string ToSql(
        ColumnDefinition column,
        FilterDefinition filter,
        string parameterName,
        DynamicParameters parameters)
    {
        var columnName = SqlName.Identifier(column.ColumnName);

        switch (filter.Operator)
        {
            case SearchOperator.Contains:
                parameters.Add(
                    parameterName,
                    $"%{filter.Value}%");

                return $"{columnName} LIKE @{parameterName}";

            case SearchOperator.StartsWith:
                parameters.Add(
                    parameterName,
                    $"{filter.Value}%");

                return $"{columnName} LIKE @{parameterName}";

            case SearchOperator.EndsWith:
                parameters.Add(
                    parameterName,
                    $"%{filter.Value}");

                return $"{columnName} LIKE @{parameterName}";

            case SearchOperator.Equals:
                parameters.Add(
                    parameterName,
                    filter.Value);

                return $"{columnName} = @{parameterName}";

            case SearchOperator.NotEquals:
                parameters.Add(
                    parameterName,
                    filter.Value);

                return $"{columnName} <> @{parameterName}";

            case SearchOperator.In:
                parameters.Add(
                    parameterName,
                    filter.Values);

                return $"{columnName} IN @{parameterName}";

            case SearchOperator.NotIn:
                parameters.Add(
                    parameterName,
                    filter.Values);

                return $"{columnName} NOT IN @{parameterName}";

            case SearchOperator.Between:
                parameters.Add(
                    $"{parameterName}a",
                    filter.Value);

                parameters.Add(
                    $"{parameterName}b",
                    filter.SecondValue);

                return
                    $"{columnName} BETWEEN " +
                    $"@{parameterName}a AND @{parameterName}b";

            case SearchOperator.GreaterThan:
                parameters.Add(
                    parameterName,
                    filter.Value);

                return $"{columnName} > @{parameterName}";

            case SearchOperator.GreaterOrEqual:
                parameters.Add(
                    parameterName,
                    filter.Value);

                return $"{columnName} >= @{parameterName}";

            case SearchOperator.LessThan:
                parameters.Add(
                    parameterName,
                    filter.Value);

                return $"{columnName} < @{parameterName}";

            case SearchOperator.LessOrEqual:
                parameters.Add(
                    parameterName,
                    filter.Value);

                return $"{columnName} <= @{parameterName}";

            case SearchOperator.IsNull:
                return $"{columnName} IS NULL";

            case SearchOperator.IsNotNull:
                return $"{columnName} IS NOT NULL";

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(filter),
                    filter.Operator,
                    "Unsupported search operator.");
        }
    }
}
