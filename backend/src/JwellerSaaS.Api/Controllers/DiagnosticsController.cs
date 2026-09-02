using JwellerSaaS.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Orion.Framework.Diagnostics;

namespace JwellerSaaS.Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
public sealed class DiagnosticsController(IOrionDiagnosticsService diagnosticsService, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<OrionDiagnosticsSnapshot>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ApiResponse<OrionDiagnosticsSnapshot>> Get()
    {
        if (!environment.IsDevelopment()) return NotFound();
        return Ok(ApiResponse<OrionDiagnosticsSnapshot>.Ok(diagnosticsService.GetSnapshot()));
    }

    [HttpGet("{entity}")]
    [ProducesResponseType(typeof(ApiResponse<MasterDiagnostics>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MasterDiagnostics>), StatusCodes.Status404NotFound)]
    public ActionResult<ApiResponse<MasterDiagnostics>> GetEntity(string entity)
    {
        if (!environment.IsDevelopment()) return NotFound();
        if (!diagnosticsService.TryGetEntity(entity, out var diagnostics) || diagnostics is null)
        {
            return NotFound(ApiResponse<MasterDiagnostics>.Fail(new ErrorResponse("metadata.not_found", $"Orion metadata entity '{entity}' was not found.")));
        }
        return Ok(ApiResponse<MasterDiagnostics>.Ok(diagnostics));
    }
}
