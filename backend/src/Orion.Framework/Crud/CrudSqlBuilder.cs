using System.Text;
using Dapper;
using Orion.Framework.Metadata;
using Orion.Framework.Query;
using Orion.Framework.Search;
using Orion.Framework.Sql;

namespace Orion.Framework.Crud;

/// <summary>Builds prepared SQL for CRUD and query specifications.</summary>
public sealed class CrudSqlBuilder
{
    /// <summary>Builds a select statement.</summary>
    public SqlStatement Select(MasterDefinition metadata, QueryDefinition query)
    {
        var parameters = new DynamicParameters();
        var sql = new StringBuilder("SELECT ");
        var options = query.Options ?? new QueryOptions();
        if (options.Distinct) sql.Append("DISTINCT ");
        if (options.Top is > 0) sql.Append(string.Join(", ", Columns(metadata, options.Columns ?? options.Projection).Take(options.Top.Value)));
        else sql.Append(string.Join(", ", Columns(metadata, options.Columns ?? options.Projection)));
        sql.Append(" FROM ").Append(SqlName.Identifier(metadata.TableName));
        AppendWhere(sql, metadata, query, parameters);
        AppendSort(sql, metadata, query.Sorts);
        if (options.PageSize is > 0) { parameters.Add("PageSize", options.PageSize.Value); parameters.Add("Offset", options.Offset ?? 0); sql.Append(" LIMIT @PageSize OFFSET @Offset"); }
        return new SqlStatement(sql.ToString(), parameters);
    }

    /// <summary>Builds a count statement.</summary>
    public SqlStatement Count(MasterDefinition metadata, QueryDefinition query)
    {
        var parameters = new DynamicParameters(); var sql = new StringBuilder("SELECT COUNT(1) FROM ").Append(SqlName.Identifier(metadata.TableName)); AppendWhere(sql, metadata, query, parameters); return new SqlStatement(sql.ToString(), parameters);
    }

    internal static void AppendPredicate(StringBuilder sql, ColumnDefinition column, SearchOperator op, string name, object? value, object? second, IReadOnlyList<object?>? values, DynamicParameters parameters)
    {
        var col = SqlName.Identifier(column.ColumnName);
        switch (op)
        {
            case SearchOperator.Contains: parameters.Add(name, $"%{value}%"); sql.Append(col).Append(" LIKE @").Append(name); break;
            case SearchOperator.StartsWith: parameters.Add(name, $"{value}%"); sql.Append(col).Append(" LIKE @").Append(name); break;
            case SearchOperator.EndsWith: parameters.Add(name, $"%{value}"); sql.Append(col).Append(" LIKE @").Append(name); break;
            case SearchOperator.Equals: parameters.Add(name, value); sql.Append(col).Append(" = @").Append(name); break;
            case SearchOperator.NotEquals: parameters.Add(name, value); sql.Append(col).Append(" <> @").Append(name); break;
            case SearchOperator.GreaterThan: parameters.Add(name, value); sql.Append(col).Append(" > @").Append(name); break;
            case SearchOperator.GreaterOrEqual: parameters.Add(name, value); sql.Append(col).Append(" >= @").Append(name); break;
            case SearchOperator.LessThan: parameters.Add(name, value); sql.Append(col).Append(" < @").Append(name); break;
            case SearchOperator.LessOrEqual: parameters.Add(name, value); sql.Append(col).Append(" <= @").Append(name); break;
            case SearchOperator.Between: parameters.Add(name + "A", value); parameters.Add(name + "B", second); sql.Append(col).Append(" BETWEEN @").Append(name).Append('A').Append(" AND @").Append(name).Append('B'); break;
            case SearchOperator.In: parameters.Add(name, values ?? Array.Empty<object?>()); sql.Append(col).Append(" IN @").Append(name); break;
            case SearchOperator.NotIn: parameters.Add(name, values ?? Array.Empty<object?>()); sql.Append(col).Append(" NOT IN @").Append(name); break;
            case SearchOperator.IsNull: sql.Append(col).Append(" IS NULL"); break;
            case SearchOperator.IsNotNull: sql.Append(col).Append(" IS NOT NULL"); break;
            default: throw new ArgumentOutOfRangeException(nameof(op));
        }
    }

    private static IEnumerable<string> Columns(MasterDefinition m, IReadOnlyList<string>? selected) => (selected is { Count: > 0 } ? selected.Select(s => Find(m, s)) : m.Columns).Select(c => SqlName.Identifier(c.ColumnName));
    private static ColumnDefinition Find(MasterDefinition m, string field) => m.Columns.First(c => c.PropertyName == field || c.ColumnName == field);
    private static void AppendWhere(StringBuilder sql, MasterDefinition m, QueryDefinition q, DynamicParameters p)
    {
        var parts = new List<string>(); var i = 0;
        foreach (var f in q.Filters ?? Array.Empty<FilterDefinition>()) { var b = new StringBuilder(); AppendPredicate(b, Find(m, f.Field), f.Operator, "p" + i++, f.Value, f.SecondValue, f.Values, p); parts.Add(b.ToString()); }
        if (!string.IsNullOrWhiteSpace(q.Search?.Term)) foreach (var c in (q.Search.Fields is { Count: > 0 } ? q.Search.Fields.Select(f => Find(m, f)) : m.Search.Columns)) { var b = new StringBuilder(); AppendPredicate(b, c, q.Search.Operator, "s" + i++, q.Search.Term, null, null, p); parts.Add(b.ToString()); }
        if (parts.Count > 0) sql.Append(" WHERE ").Append(string.Join(" AND ", parts));
    }
    private static void AppendSort(StringBuilder sql, MasterDefinition m, IReadOnlyList<SortDefinition>? sorts) { if (sorts is not { Count: > 0 }) return; sql.Append(" ORDER BY ").Append(string.Join(", ", sorts.Select(s => SqlName.Identifier(Find(m, s.Field).ColumnName) + (s.Direction == SortDirection.Descending ? " DESC" : " ASC")))); }
}
