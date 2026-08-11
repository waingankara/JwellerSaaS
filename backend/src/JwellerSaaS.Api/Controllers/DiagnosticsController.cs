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
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        return Ok(ApiResponse<OrionDiagnosticsSnapshot>.Ok(diagnosticsService.GetSnapshot()));
    }
}
