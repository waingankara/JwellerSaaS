using System.Text.Json;
using System.Text.Json.Nodes;
using JwellerSaaS.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Orion.Framework.Crud;

namespace JwellerSaaS.Api.Controllers.Masters;

[ApiController]
[Route("api/master/{master}")]
public sealed class MasterApiController(IGenericMasterCrudService masters) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create(string master, JsonElement request, CancellationToken cancellationToken) =>
        ApiResponse<object>.Ok(await masters.CreateAsync(master, request, cancellationToken).ConfigureAwait(false));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<object>>> Get(string master, long id, CancellationToken cancellationToken)
    {
        var entity = await masters.GetAsync(master, id, cancellationToken).ConfigureAwait(false);
        return entity is null ? NotFound(ApiResponse<object>.Fail(new("MASTER_NOT_FOUND", "Master row not found."))) : ApiResponse<object>.Ok(entity);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(string master, long id, JsonElement request, CancellationToken cancellationToken) =>
        ApiResponse<object>.Ok(await masters.UpdateAsync(master, id, request, cancellationToken).ConfigureAwait(false));

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<int>>> Delete(string master, long id, CancellationToken cancellationToken) =>
        ApiResponse<int>.Ok(await masters.DeleteAsync(master, id, cancellationToken).ConfigureAwait(false));

    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<object>>>> Search(string master, JsonElement request, CancellationToken cancellationToken) =>
        ApiResponse<IReadOnlyList<object>>.Ok(await masters.SearchAsync(master, request, cancellationToken).ConfigureAwait(false));

    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<long>>> Count(string master, CancellationToken cancellationToken) =>
        ApiResponse<long>.Ok(await masters.CountAsync(master, QueryToJson(), cancellationToken).ConfigureAwait(false));

    [HttpGet("{id:long}/exists")]
    public async Task<ActionResult<ApiResponse<bool>>> Exists(string master, long id, CancellationToken cancellationToken) =>
        ApiResponse<bool>.Ok(await masters.ExistsAsync(master, id, cancellationToken).ConfigureAwait(false));

    [HttpPatch("{id:long}/soft-delete")]
    public async Task<ActionResult<ApiResponse<int>>> SoftDelete(string master, long id, CancellationToken cancellationToken) =>
        ApiResponse<int>.Ok(await masters.SoftDeleteAsync(master, id, cancellationToken).ConfigureAwait(false));

    [HttpPatch("{id:long}/restore")]
    public async Task<ActionResult<ApiResponse<int>>> Restore(string master, long id, CancellationToken cancellationToken) =>
        ApiResponse<int>.Ok(await masters.RestoreAsync(master, id, cancellationToken).ConfigureAwait(false));

    [HttpGet("dropdown")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<object>>>> Dropdown(string master, [FromQuery] bool activeOnly, [FromQuery] string? searchText, [FromQuery] int top, CancellationToken cancellationToken) =>
        ApiResponse<IReadOnlyList<object>>.Ok(await masters.DropdownAsync(master, activeOnly, searchText, top <= 0 ? 20 : top, cancellationToken).ConfigureAwait(false));

    private JsonElement QueryToJson()
    {
        var json = new JsonObject();
        foreach (var item in Request.Query)
        {
            var value = item.Value.ToString();
            if (bool.TryParse(value, out var boolValue)) json[item.Key] = boolValue;
            else if (int.TryParse(value, out var intValue)) json[item.Key] = intValue;
            else json[item.Key] = value;
        }

        return JsonSerializer.Deserialize<JsonElement>(json.ToJsonString());
    }
}
