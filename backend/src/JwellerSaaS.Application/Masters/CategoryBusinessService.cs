using JwellerSaaS.Contracts.Masters;
using JwellerSaaS.Domain.Masters;
using Orion.Framework.Crud;
using Orion.Framework.Data;
using Orion.Framework.Query;
using Orion.Framework.Search;
using Orion.Framework.Tenancy;

namespace JwellerSaaS.Application.Masters;

public interface ICategoryBusinessService
{
    Task<Category> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<Category?> GetAsync(long id, CancellationToken cancellationToken);
    Task<Category> UpdateAsync(long id, UpdateCategoryRequest request, CancellationToken cancellationToken);
    Task<int> DeleteAsync(long id, CancellationToken cancellationToken);
    Task<int> SoftDeleteAsync(long id, CancellationToken cancellationToken);
    Task<int> RestoreAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Category>> SearchAsync(CategorySearchRequest request, CancellationToken cancellationToken);
    Task<long> CountAsync(CategorySearchRequest request, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Category>> DropdownAsync(bool activeOnly, string? searchText, int top, CancellationToken cancellationToken);
}

public sealed class CategoryBusinessService(ICrudService<Category> crud, ISqlExecutor executor, ITenantContextAccessor tenantAccessor) : ICategoryBusinessService
{
    public async Task<Category> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        Validate(request.CategoryCode, request.CategoryName, request.DisplayOrder);
        await EnsureUniqueAsync(request.CategoryCode, request.CategoryName, null, false, cancellationToken).ConfigureAwait(false);
        return await crud.CreateAsync(new Category { CategoryCode = request.CategoryCode.Trim(), CategoryName = request.CategoryName.Trim(), DisplayOrder = request.DisplayOrder, Remarks = request.Remarks, IsActive = request.IsActive, RowVersion = Guid.NewGuid() }, cancellationToken).ConfigureAwait(false);
    }

    public Task<Category?> GetAsync(long id, CancellationToken cancellationToken) => crud.GetByIdAsync(id, cancellationToken);

    public async Task<Category> UpdateAsync(long id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        Validate(request.CategoryCode, request.CategoryName, request.DisplayOrder);
        var existing = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        await EnsureUniqueAsync(request.CategoryCode, request.CategoryName, id, false, cancellationToken).ConfigureAwait(false);
        existing.CategoryCode = request.CategoryCode.Trim();
        existing.CategoryName = request.CategoryName.Trim();
        existing.DisplayOrder = request.DisplayOrder;
        existing.Remarks = request.Remarks;
        existing.IsActive = request.IsActive;
        existing.RowVersion = Guid.NewGuid();
        return await crud.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var existing = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing.IsSystem) throw new InvalidOperationException("Cannot delete system category.");
        return await crud.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> SoftDeleteAsync(long id, CancellationToken cancellationToken)
    {
        var existing = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing.IsSystem) throw new InvalidOperationException("Cannot delete system category.");
        return await crud.SoftDeleteAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> RestoreAsync(long id, CancellationToken cancellationToken)
    {
        var existing = await GetRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        await EnsureUniqueAsync(existing.CategoryCode, existing.CategoryName, id, false, cancellationToken).ConfigureAwait(false);
        return await crud.RestoreAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyList<Category>> SearchAsync(CategorySearchRequest request, CancellationToken cancellationToken) => crud.GetManyAsync(BuildQuery(request), cancellationToken);
    public Task<long> CountAsync(CategorySearchRequest request, CancellationToken cancellationToken) => crud.CountAsync(BuildQuery(request) with { Options = null }, cancellationToken);
    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken) => crud.ExistsAsync(id, cancellationToken);

    public Task<IReadOnlyList<Category>> DropdownAsync(bool activeOnly, string? searchText, int top, CancellationToken cancellationToken)
    {
        var filters = new List<FilterDefinition>();
        if (activeOnly) filters.Add(new FilterDefinition(nameof(Category.IsActive), SearchOperator.Equals, true));
        filters.Add(new FilterDefinition(nameof(Category.IsDeleted), SearchOperator.Equals, false));
        return crud.GetManyAsync(new QueryDefinition(filters, [new SortDefinition(nameof(Category.DisplayOrder)), new SortDefinition(nameof(Category.CategoryName))], new QueryOptions(Top: Math.Clamp(top, 1, 100)), string.IsNullOrWhiteSpace(searchText) ? null : new SearchOptions(searchText, [nameof(Category.CategoryName)])), cancellationToken);
    }

    private async Task<Category> GetRequiredAsync(long id, CancellationToken cancellationToken) => await crud.GetByIdAsync(id, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Category Not Found");

    private async Task EnsureUniqueAsync(string code, string name, long? exceptId, bool includeDeleted, CancellationToken cancellationToken)
    {
        var sql = "select count(1) from category where tenant_id = @TenantId and (@IncludeDeleted or is_deleted = false) and (@ExceptId is null or category_id <> @ExceptId) and (upper(category_code) = upper(@Code) or upper(category_name) = upper(@Name))";
        var count = await executor.ScalarAsync<long>(sql, new { tenantAccessor.TenantContext.TenantId, IncludeDeleted = includeDeleted, ExceptId = exceptId, Code = code.Trim(), Name = name.Trim() }, cancellationToken).ConfigureAwait(false);
        if (count > 0) throw new InvalidOperationException("Duplicate Category Code or Duplicate Category Name");
    }

    private static void Validate(string code, string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Category Code required.");
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Category Name required.");
        if (code.Length > 20) throw new InvalidOperationException("Maximum Code Length exceeded.");
        if (name.Length > 200) throw new InvalidOperationException("Maximum Name Length exceeded.");
        if (displayOrder < 0) throw new InvalidOperationException("Display Order must be greater than or equal to 0.");
    }

    private static QueryDefinition BuildQuery(CategorySearchRequest request)
    {
        var filters = new List<FilterDefinition>();
        if (!string.IsNullOrWhiteSpace(request.CategoryCode)) filters.Add(new FilterDefinition(nameof(Category.CategoryCode), SearchOperator.Contains, request.CategoryCode));
        if (!string.IsNullOrWhiteSpace(request.CategoryName)) filters.Add(new FilterDefinition(nameof(Category.CategoryName), SearchOperator.Contains, request.CategoryName));
        if (request.IsActive.HasValue) filters.Add(new FilterDefinition(nameof(Category.IsActive), SearchOperator.Equals, request.IsActive.Value));
        if (request.IsDeleted.HasValue) filters.Add(new FilterDefinition(nameof(Category.IsDeleted), SearchOperator.Equals, request.IsDeleted.Value));
        if (request.CreatedFrom.HasValue) filters.Add(new FilterDefinition(nameof(Category.CreatedDate), SearchOperator.GreaterOrEqual, request.CreatedFrom.Value));
        if (request.CreatedTo.HasValue) filters.Add(new FilterDefinition(nameof(Category.CreatedDate), SearchOperator.LessOrEqual, request.CreatedTo.Value));
        var sort = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(Category.CategoryName) : request.SortBy;
        return new QueryDefinition(filters, [new SortDefinition(sort, request.Descending ? SortDirection.Descending : SortDirection.Ascending)], new QueryOptions((Math.Max(1, request.Page) - 1) * Math.Clamp(request.PageSize, 1, 100), Math.Clamp(request.PageSize, 1, 100)), string.IsNullOrWhiteSpace(request.Keyword) ? null : new SearchOptions(request.Keyword, [nameof(Category.CategoryCode), nameof(Category.CategoryName)]));
    }
}
