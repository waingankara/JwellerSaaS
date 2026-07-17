namespace Orion.Framework.Pagination;

/// <summary>Represents a paged query request.</summary>
public sealed record PagedRequest(int PageNumber, int PageSize, IReadOnlyList<SortDefinition> Sorts, IReadOnlyList<FilterDefinition> Filters)
{
    /// <summary>Gets the zero-based row offset.</summary>
    public int Offset => (Math.Max(1, PageNumber) - 1) * Math.Max(1, PageSize);
}

/// <summary>Represents a paged query result.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize)
{
    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>Defines a sort expression.</summary>
public sealed record SortDefinition(string Field, SortDirection Direction);

/// <summary>Defines a filter expression.</summary>
public sealed record FilterDefinition(string Field, Search.SearchOperator Operator, object? Value = null, object? SecondValue = null, IReadOnlyList<object>? Values = null);

/// <summary>Defines sort direction.</summary>
public enum SortDirection { Ascending, Descending }
