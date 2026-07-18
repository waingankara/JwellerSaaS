namespace JwellerSaaS.Contracts.Masters;

public sealed record CreateCategoryRequest(string CategoryCode, string CategoryName, int DisplayOrder = 0, string? Remarks = null, bool IsActive = true);
public sealed record UpdateCategoryRequest(string CategoryCode, string CategoryName, int DisplayOrder = 0, string? Remarks = null, bool IsActive = true, Guid? RowVersion = null);
public sealed record CategoryResponse(long CategoryId, long TenantId, string CategoryCode, string CategoryName, int DisplayOrder, string? Remarks, bool IsActive, bool IsDeleted, DateTimeOffset? CreatedDate, DateTimeOffset? ModifiedDate, Guid RowVersion);
public sealed record CategorySearchRequest(string? CategoryCode = null, string? CategoryName = null, bool? IsActive = null, DateTimeOffset? CreatedFrom = null, DateTimeOffset? CreatedTo = null, bool? IsDeleted = false, string? Keyword = null, int Page = 1, int PageSize = 20, string? SortBy = "CategoryName", bool Descending = false);
public sealed record CategoryDropdownResponse(long CategoryId, string CategoryName, int DisplayOrder);
