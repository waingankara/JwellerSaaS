using JwellerSaaS.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using Orion.Framework.Options;

namespace JwellerSaaS.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(IConfiguration configuration, IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Get(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var postgres = false;
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                postgres = true;
            }
            catch (NpgsqlException)
            {
                postgres = false;
            }
        }

        var configurationValid = !string.IsNullOrWhiteSpace(jwtOptions.Value.Issuer) && !string.IsNullOrWhiteSpace(jwtOptions.Value.Audience) && jwtOptions.Value.SigningKey.Length >= 32;
        return Ok(ApiResponse<object>.Ok(new { status = "Healthy", postgres, configurationValid, ready = configurationValid }));
    }
}
