using Dapper;
using Orion.Framework.Metadata;
using Orion.Framework.Pagination;
using Orion.Framework.Search;
using Orion.Framework.Query;
using FilterDefinition = Orion.Framework.Query.FilterDefinition;

namespace Orion.Framework.Sql;

/// <summary>Builds parameterized insert statements.</summary>
public sealed class InsertBuilder { public SqlStatement Build(MasterDefinition d, object values) { var cols=d.Columns.Where(c=>!c.IsPrimaryKey).ToArray(); return new SqlStatement($"INSERT INTO {SqlName.Identifier(d.TableName)} ({string.Join(", ", cols.Select(c=>SqlName.Identifier(c.ColumnName)))}) VALUES ({string.Join(", ", cols.Select(c=>"@"+c.PropertyName))})", values); } }
/// <summary>Builds parameterized update statements.</summary>
public sealed class UpdateBuilder { public SqlStatement Build(MasterDefinition d, object values) { var key=d.Columns.First(c=>c.IsPrimaryKey); var cols=d.Columns.Where(c=>!c.IsPrimaryKey).ToArray(); return new SqlStatement($"UPDATE {SqlName.Identifier(d.TableName)} SET {string.Join(", ", cols.Select(c=>$"{SqlName.Identifier(c.ColumnName)} = @{c.PropertyName}"))} WHERE {SqlName.Identifier(key.ColumnName)} = @{key.PropertyName}", values); } }
/// <summary>Builds parameterized delete statements.</summary>
public sealed class DeleteBuilder { public SqlStatement Build(MasterDefinition d, object parameters) { var key=d.Columns.First(c=>c.IsPrimaryKey); return new SqlStatement($"DELETE FROM {SqlName.Identifier(d.TableName)} WHERE {SqlName.Identifier(key.ColumnName)} = @{key.PropertyName}", parameters); } }
/// <summary>Builds parameterized select statements.</summary>
public sealed class SelectBuilder { public SqlStatement Build(MasterDefinition d, object? parameters=null) => new($"SELECT {string.Join(", ", d.Columns.Select(c=>SqlName.Identifier(c.ColumnName)))} FROM {SqlName.Identifier(d.TableName)}", parameters); }
/// <summary>Builds parameterized existence statements.</summary>
public sealed class ExistsBuilder { public SqlStatement Build(MasterDefinition d) { var key=d.Columns.First(c=>c.IsPrimaryKey); return new SqlStatement($"SELECT EXISTS (SELECT 1 FROM {SqlName.Identifier(d.TableName)} WHERE {SqlName.Identifier(key.ColumnName)} = @{key.PropertyName})", null); } }
/// <summary>Builds parameterized count statements.</summary>
public sealed class CountBuilder { public SqlStatement Build(MasterDefinition d) => new($"SELECT COUNT(1) FROM {SqlName.Identifier(d.TableName)}", null); }
/// <summary>Builds duplicate check statements.</summary>
public sealed class DuplicateBuilder { public SqlStatement Build(MasterDefinition d, object values) { var cols=d.Duplicate.Columns; if (cols.Count==0) throw new InvalidOperationException("Duplicate check requires duplicate columns."); return new SqlStatement($"SELECT EXISTS (SELECT 1 FROM {SqlName.Identifier(d.TableName)} WHERE {string.Join(" AND ", cols.Select(c=>$"{SqlName.Identifier(c.ColumnName)} = @{c.PropertyName}"))})", values); } }
/// <summary>Builds pagination clauses.</summary>
public sealed class PaginationBuilder { public string Build(PagedRequest request) => $"LIMIT @PageSize OFFSET @Offset"; }
/// <summary>Builds parameterized search predicates.</summary>
public sealed class SearchBuilder
{
    /// <summary>Builds a WHERE clause from filters.</summary>
    public SqlStatement Build(MasterDefinition d, IReadOnlyList<FilterDefinition> filters)
    {
        var p = new DynamicParameters(); var parts = new List<string>(); var i=0;
        foreach (var f in filters) { var c=d.Columns.First(x=>x.PropertyName==f.Field || x.ColumnName==f.Field); var name="p"+i++; parts.Add(ToSql(c, f, name, p)); }
        return new SqlStatement(parts.Count==0 ? string.Empty : "WHERE "+string.Join(" AND ", parts), p);
    }
    private static string ToSql(ColumnDefinition c, FilterDefinition f, string p, DynamicParameters ps) { var col=SqlName.Identifier(c.ColumnName); switch(f.Operator) { case SearchOperator.Contains: ps.Add(p,$"%{f.Value}%"); return $"{col} LIKE @{p}"; case SearchOperator.StartsWith: ps.Add(p,$"{f.Value}%"); return $"{col} LIKE @{p}"; case SearchOperator.EndsWith: ps.Add(p,$"%{f.Value}"); return $"{col} LIKE @{p}"; case SearchOperator.Equals: ps.Add(p,f.Value); return $"{col} = @{p}"; case SearchOperator.NotEquals: ps.Add(p,f.Value); return $"{col} <> @{p}"; case SearchOperator.In: ps.Add(p,f.Values); return $"{col} IN @{p}"; case SearchOperator.NotIn: ps.Add(p,f.Values); return $"{col} NOT IN @{p}"; case SearchOperator.Between: ps.Add(p+"a",f.Value); ps.Add(p+"b",f.SecondValue); return $"{col} BETWEEN @{p}a AND @{p}b"; case SearchOperator.GreaterThan: ps.Add(p,f.Value); return $"{col} > @{p}"; case SearchOperator.GreaterOrEqual: ps.Add(p,f.Value); return $"{col} >= @{p}"; case SearchOperator.LessThan: ps.Add(p,f.Value); return $"{col} < @{p}"; case SearchOperator.LessOrEqual: ps.Add(p,f.Value); return $"{col} <= @{p}"; case SearchOperator.IsNull: return $"{col} IS NULL"; case SearchOperator.IsNotNull: return $"{col} IS NOT NULL"; default: throw new ArgumentOutOfRangeException(nameof(f)); } }
}
