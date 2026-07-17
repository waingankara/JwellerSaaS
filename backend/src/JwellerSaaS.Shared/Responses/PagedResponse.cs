namespace JwellerSaaS.Shared.Responses;

/// <summary>Represents a paged API response.</summary>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, long TotalCount)
{
    /// <summary>Gets the total number of pages.</summary>
    public long TotalPages => PageSize <= 0 ? 0 : (long)Math.Ceiling(TotalCount / (double)PageSize);
}
