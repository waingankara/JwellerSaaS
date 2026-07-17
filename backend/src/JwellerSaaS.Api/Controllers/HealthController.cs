using JwellerSaaS.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JwellerSaaS.Api.Controllers;

/// <summary>Provides lightweight operational health endpoints.</summary>
[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    /// <summary>Returns API liveness status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<string>> Get() => Ok(ApiResponse<string>.Ok("Healthy"));
}
