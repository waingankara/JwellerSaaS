using Orion.Framework.Search;

namespace Orion.Framework.Query;

/// <summary>Defines a filter over an entity field.</summary>
public sealed record FilterDefinition(string Field, SearchOperator Operator, object? Value = null, object? SecondValue = null, IReadOnlyList<object?>? Values = null);

/// <summary>Defines a sort expression.</summary>
public sealed record SortDefinition(string Field, SortDirection Direction = SortDirection.Ascending);

/// <summary>Sort direction.</summary>
public enum SortDirection {
    /// <summary>Ascending.</summary>
Ascending,
    /// <summary>Descending.</summary>
Descending }

/// <summary>Search text and target fields.</summary>
public sealed record SearchOptions(string? Term, IReadOnlyList<string>? Fields = null, SearchOperator Operator = SearchOperator.Contains);

/// <summary>Additional query behavior.</summary>
public sealed record QueryOptions(int? Offset = null, int? PageSize = null, string? Cursor = null, bool Distinct = false, int? Top = null, IReadOnlyList<string>? Columns = null, IReadOnlyList<string>? Projection = null);

/// <summary>Complete query specification consumed by the CRUD engine.</summary>
public sealed record QueryDefinition(IReadOnlyList<FilterDefinition>? Filters = null, IReadOnlyList<SortDefinition>? Sorts = null, QueryOptions? Options = null, SearchOptions? Search = null)
{
    /// <summary>Empty query definition.</summary>
    public static QueryDefinition Empty { get; } = new();
}
