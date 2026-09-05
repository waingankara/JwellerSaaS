using JwellerSaaS.Application.Identity;
using JwellerSaaS.Contracts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwellerSaaS.Api.Controllers;

[ApiController]
[Route("api/identity")]
public sealed class IdentityController(
    IIdentityService identityService)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await identityService.LoginAsync(
                request,
                cancellationToken);

        return Ok(response);
    }
}
