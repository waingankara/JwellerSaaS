using System.Text;
using Dapper;
using Orion.Framework.Metadata;
using Orion.Framework.Query;
using Orion.Framework.Search;
using Orion.Framework.Sql;

namespace Orion.Framework.Crud;

/// <summary>
/// Builds prepared SQL statements for CRUD and query specifications.
/// </summary>
public sealed class CrudSqlBuilder
{
    /// <summary>
    /// Builds a SELECT statement.
    /// </summary>
    public SqlStatement Select(
        MasterDefinition metadata,
        QueryDefinition query)
    {
        var parameters = new DynamicParameters();
        var sql = new StringBuilder("SELECT ");

        var options =
            query.Options ?? new QueryOptions();

        AppendDistinct(sql, options);
        AppendColumns(sql, metadata, options);

        sql.Append(" FROM ");
        sql.Append(
            SqlName.Identifier(metadata.TableName));

        AppendWhere(
            sql,
            metadata,
            query,
            parameters);

        AppendSort(
            sql,
            metadata,
            query.Sorts);

        AppendPagination(
            sql,
            options,
            parameters);

        return new SqlStatement(
            sql.ToString(),
            parameters);
    }

    /// <summary>
    /// Builds a COUNT statement.
    /// </summary>
    public SqlStatement Count(
        MasterDefinition metadata,
        QueryDefinition query)
    {
        var parameters = new DynamicParameters();

        var sql = new StringBuilder(
            "SELECT COUNT(1) FROM ");

        sql.Append(
            SqlName.Identifier(metadata.TableName));

        AppendWhere(
            sql,
            metadata,
            query,
            parameters);

        return new SqlStatement(
            sql.ToString(),
            parameters);
    }

    /// <summary>
    /// Appends a search predicate to the SQL statement.
    /// </summary>
    internal static void AppendPredicate(
        StringBuilder sql,
        ColumnDefinition column,
        SearchOperator op,
        string name,
        object? value,
        object? second,
        IReadOnlyList<object?>? values,
        DynamicParameters parameters)
    {
        var columnName =
            SqlName.Identifier(column.ColumnName);

        switch (op)
        {
            case SearchOperator.Contains:
                parameters.Add(
                    name,
                    $"%{value}%");

                sql.Append(columnName)
                    .Append(" LIKE @")
                    .Append(name);

                break;

            case SearchOperator.StartsWith:
                parameters.Add(
                    name,
                    $"{value}%");

                sql.Append(columnName)
                    .Append(" LIKE @")
                    .Append(name);

                break;

            case SearchOperator.EndsWith:
                parameters.Add(
                    name,
                    $"%{value}");

                sql.Append(columnName)
                    .Append(" LIKE @")
                    .Append(name);

                break;

            case SearchOperator.Equals:
                parameters.Add(
                    name,
                    value);

                sql.Append(columnName)
                    .Append(" = @")
                    .Append(name);

                break;

            case SearchOperator.NotEquals:
                parameters.Add(
                    name,
                    value);

                sql.Append(columnName)
                    .Append(" <> @")
                    .Append(name);

                break;

            case SearchOperator.GreaterThan:
                parameters.Add(
                    name,
                    value);

                sql.Append(columnName)
                    .Append(" > @")
                    .Append(name);

                break;

            case SearchOperator.GreaterOrEqual:
                parameters.Add(
                    name,
                    value);

                sql.Append(columnName)
                    .Append(" >= @")
                    .Append(name);

                break;

            case SearchOperator.LessThan:
                parameters.Add(
                    name,
                    value);

                sql.Append(columnName)
                    .Append(" < @")
                    .Append(name);

                break;

            case SearchOperator.LessOrEqual:
                parameters.Add(
                    name,
                    value);

                sql.Append(columnName)
                    .Append(" <= @")
                    .Append(name);

                break;

            case SearchOperator.Between:
                parameters.Add(
                    name + "A",
                    value);

                parameters.Add(
                    name + "B",
                    second);

                sql.Append(columnName)
                    .Append(" BETWEEN @")
                    .Append(name)
                    .Append('A')
                    .Append(" AND @")
                    .Append(name)
                    .Append('B');

                break;

            case SearchOperator.In:
                parameters.Add(
                    name,
                    values ?? Array.Empty<object?>());

                sql.Append(columnName)
                    .Append(" IN @")
                    .Append(name);

                break;

            case SearchOperator.NotIn:
                parameters.Add(
                    name,
                    values ?? Array.Empty<object?>());

                sql.Append(columnName)
                    .Append(" NOT IN @")
                    .Append(name);

                break;

            case SearchOperator.IsNull:
                sql.Append(columnName)
                    .Append(" IS NULL");

                break;

            case SearchOperator.IsNotNull:
                sql.Append(columnName)
                    .Append(" IS NOT NULL");

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(op));
        }
    }

    private static IEnumerable<string> Columns(
        MasterDefinition metadata,
        IReadOnlyList<string>? selected)
    {
        var columns =
            selected is { Count: > 0 }
                ? selected.Select(
                    field => Find(metadata, field))
                : metadata.Columns;

        return columns.Select(
            column =>
                SqlName.Identifier(
                    column.ColumnName));
    }

    private static ColumnDefinition Find(
        MasterDefinition metadata,
        string field)
    {
        return metadata.Columns.First(
            column =>
                string.Equals(
                    column.PropertyName,
                    field,
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    column.ColumnName,
                    field,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static void AppendWhere(
        StringBuilder sql,
        MasterDefinition metadata,
        QueryDefinition query,
        DynamicParameters parameters)
    {
        var conditions = new List<string>();
        var parameterIndex = 0;

        AppendFilters(
            metadata,
            query,
            parameters,
            conditions,
            ref parameterIndex);

        AppendSearch(
            metadata,
            query,
            parameters,
            conditions,
            ref parameterIndex);

        if (conditions.Count == 0)
        {
            return;
        }

        sql.Append(" WHERE ");
        sql.Append(
            string.Join(
                " AND ",
                conditions));
    }

    private static void AppendFilters(
        MasterDefinition metadata,
        QueryDefinition query,
        DynamicParameters parameters,
        List<string> conditions,
        ref int parameterIndex)
    {
        foreach (var filter in
                 query.Filters ?? Array.Empty<FilterDefinition>())
        {
            var predicate =
                new StringBuilder();

            AppendPredicate(
                predicate,
                Find(
                    metadata,
                    filter.Field),
                filter.Operator,
                "p" + parameterIndex++,
                filter.Value,
                filter.SecondValue,
                filter.Values,
                parameters);

            conditions.Add(
                predicate.ToString());
        }
    }

    private static void AppendSearch(
        MasterDefinition metadata,
        QueryDefinition query,
        DynamicParameters parameters,
        List<string> conditions,
        ref int parameterIndex)
    {
        if (string.IsNullOrWhiteSpace(
                query.Search?.Term))
        {
            return;
        }

        var searchColumns =
            query.Search.Fields is { Count: > 0 }
                ? query.Search.Fields.Select(
                    field => Find(metadata, field))
                : metadata.Search.Columns;

        foreach (var column in searchColumns)
        {
            var predicate =
                new StringBuilder();

            AppendPredicate(
                predicate,
                column,
                query.Search.Operator,
                "s" + parameterIndex++,
                query.Search.Term,
                null,
                null,
                parameters);

            conditions.Add(
                predicate.ToString());
        }
    }

    private static void AppendSort(
        StringBuilder sql,
        MasterDefinition metadata,
        IReadOnlyList<SortDefinition>? sorts)
    {
        if (sorts is not { Count: > 0 })
        {
            return;
        }

        var orderBy =
            sorts.Select(
                sort =>
                {
                    var column =
                        Find(
                            metadata,
                            sort.Field);

                    var direction =
                        sort.Direction ==
                        SortDirection.Descending
                            ? " DESC"
                            : " ASC";

                    return
                        SqlName.Identifier(
                            column.ColumnName) +
                        direction;
                });

        sql.Append(" ORDER BY ");
        sql.Append(
            string.Join(
                ", ",
                orderBy));
    }

    private static void AppendDistinct(
        StringBuilder sql,
        QueryOptions options)
    {
        if (options.Distinct)
        {
            sql.Append("DISTINCT ");
        }
    }

    private static void AppendColumns(
        StringBuilder sql,
        MasterDefinition metadata,
        QueryOptions options)
    {
        var selectedColumns =
            options.Columns ??
            options.Projection;

        var columns =
            Columns(
                metadata,
                selectedColumns);

        if (options.Top is > 0)
        {
            columns = columns.Take(
                options.Top.Value);
        }

        sql.Append(
            string.Join(
                ", ",
                columns));
    }

    private static void AppendPagination(
        StringBuilder sql,
        QueryOptions options,
        DynamicParameters parameters)
    {
        if (options.PageSize is > 0)
        {
            parameters.Add(
                "PageSize",
                options.PageSize.Value);

            parameters.Add(
                "Offset",
                options.Offset ?? 0);

            sql.Append(
                " LIMIT @PageSize OFFSET @Offset");

            return;
        }

        if (options.Top is > 0)
        {
            parameters.Add(
                "Top",
                options.Top.Value);

            sql.Append(
                " LIMIT @Top");
        }
    }
}
