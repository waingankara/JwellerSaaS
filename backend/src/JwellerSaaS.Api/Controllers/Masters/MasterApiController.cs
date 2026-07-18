using JwellerSaaS.Application.Masters;
using JwellerSaaS.Contracts.Masters;
using JwellerSaaS.Domain.Masters;
using JwellerSaaS.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JwellerSaaS.Api.Controllers.Masters;

[ApiController]
[Route("api/master/{master}")]
public sealed class MasterApiController(ICategoryBusinessService categories) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Create(string master, CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        return ApiResponse<CategoryResponse>.Ok(ToResponse(await categories.CreateAsync(request, cancellationToken).ConfigureAwait(false)));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Get(string master, long id, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        var category = await categories.GetAsync(id, cancellationToken).ConfigureAwait(false);
        return category is null ? NotFound(ApiResponse<CategoryResponse>.Fail(new("CATEGORY_NOT_FOUND", "Category Not Found"))) : ApiResponse<CategoryResponse>.Ok(ToResponse(category));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Update(string master, long id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        return ApiResponse<CategoryResponse>.Ok(ToResponse(await categories.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false)));
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<int>>> Delete(string master, long id, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        return ApiResponse<int>.Ok(await categories.DeleteAsync(id, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryResponse>>>> Search(string master, CategorySearchRequest request, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        var rows = await categories.SearchAsync(request, cancellationToken).ConfigureAwait(false);
        return ApiResponse<IReadOnlyList<CategoryResponse>>.Ok(rows.Select(ToResponse).ToArray());
    }

    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<long>>> Count(string master, [FromQuery] CategorySearchRequest request, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        return ApiResponse<long>.Ok(await categories.CountAsync(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("{id:long}/exists")]
    public async Task<ActionResult<ApiResponse<bool>>> Exists(string master, long id, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        return ApiResponse<bool>.Ok(await categories.ExistsAsync(id, cancellationToken).ConfigureAwait(false));
    }

    [HttpPatch("{id:long}/soft-delete")]
    public async Task<ActionResult<ApiResponse<int>>> SoftDelete(string master, long id, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        return ApiResponse<int>.Ok(await categories.SoftDeleteAsync(id, cancellationToken).ConfigureAwait(false));
    }

    [HttpPatch("{id:long}/restore")]
    public async Task<ActionResult<ApiResponse<int>>> Restore(string master, long id, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        return ApiResponse<int>.Ok(await categories.RestoreAsync(id, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("dropdown")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDropdownResponse>>>> Dropdown(string master, [FromQuery] bool activeOnly, [FromQuery] string? searchText, [FromQuery] int top, CancellationToken cancellationToken)
    {
        if (!IsCategory(master)) return NotFound();
        var rows = await categories.DropdownAsync(activeOnly, searchText, top <= 0 ? 20 : top, cancellationToken).ConfigureAwait(false);
        return ApiResponse<IReadOnlyList<CategoryDropdownResponse>>.Ok(rows.Select(c => new CategoryDropdownResponse(c.CategoryId, c.CategoryName, c.DisplayOrder)).ToArray());
    }

    private static bool IsCategory(string master) => string.Equals(master, "category", StringComparison.OrdinalIgnoreCase);
    private static CategoryResponse ToResponse(Category c) => new(c.CategoryId, c.TenantId, c.CategoryCode, c.CategoryName, c.DisplayOrder, c.Remarks, c.IsActive, c.IsDeleted, c.CreatedDate, c.ModifiedDate, c.RowVersion);
}
