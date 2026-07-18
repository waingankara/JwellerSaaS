using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orion.Framework.Options;
using Orion.Framework.Security;

namespace Orion.Framework.Authentication;

public sealed record TokenPair(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt, DateTimeOffset RefreshTokenExpiresAt);

public interface IClaimsBuilder { IReadOnlyCollection<Claim> Build(CurrentUser user); }
public interface ITokenGenerator { TokenPair Generate(CurrentUser user); }
public interface ITokenValidator { ClaimsPrincipal Validate(string token); }

public sealed class ClaimsBuilder : IClaimsBuilder
{
    public IReadOnlyCollection<Claim> Build(CurrentUser user)
    {
        var claims = new List<Claim>();
        Add(claims, JwtRegisteredClaimNames.Sub, user.UserId?.ToString());
        Add(claims, ClaimTypes.NameIdentifier, user.UserId?.ToString());
        Add(claims, ClaimTypes.Name, user.Username);
        Add(claims, ClaimTypes.Email, user.Email);
        Add(claims, OrionClaimTypes.TenantId, user.TenantId?.ToString());
        Add(claims, OrionClaimTypes.BranchId, user.BranchId?.ToString());
        Add(claims, OrionClaimTypes.Language, user.Language);
        Add(claims, OrionClaimTypes.Timezone, user.Timezone);
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim(OrionClaimTypes.Permission, permission)));
        return claims;
    }

    private static void Add(ICollection<Claim> claims, string type, string? value) { if (!string.IsNullOrWhiteSpace(value)) claims.Add(new Claim(type, value)); }
}

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options, IClaimsBuilder claimsBuilder) : ITokenGenerator
{
    public TokenPair Generate(CurrentUser user)
    {
        var jwt = options.Value;
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(jwt.AccessTokenMinutes);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claimsBuilder.Build(user), now.UtcDateTime, expires.UtcDateTime, credentials);
        var refreshBytes = RandomNumberGenerator.GetBytes(64);
        return new TokenPair(new JwtSecurityTokenHandler().WriteToken(token), Convert.ToBase64String(refreshBytes), expires, now.AddDays(jwt.RefreshTokenDays));
    }
}

public sealed class JwtTokenValidator(IOptions<JwtOptions> options) : ITokenValidator
{
    public ClaimsPrincipal Validate(string token)
    {
        var jwt = options.Value;
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
    }
}

public static class OrionClaimTypes
{
    public const string TenantId = "tenant_id";
    public const string BranchId = "branch_id";
    public const string Permission = "permission";
    public const string Language = "language";
    public const string Timezone = "timezone";
}
