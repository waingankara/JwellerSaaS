using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orion.Framework.Options;
using Orion.Framework.Security;

namespace Orion.Framework.Authentication;

/// <summary>
/// Represents an access token and its corresponding refresh token.
/// </summary>
public sealed record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);

/// <summary>
/// Builds claims for the current user.
/// </summary>
public interface IClaimsBuilder
{
    IReadOnlyCollection<Claim> Build(
        CurrentUser user);
}

/// <summary>
/// Generates authentication tokens for the current user.
/// </summary>
public interface ITokenGenerator
{
    TokenPair Generate(
        CurrentUser user);
}

/// <summary>
/// Validates an authentication token.
/// </summary>
public interface ITokenValidator
{
    ClaimsPrincipal Validate(
        string token);
}

/// <summary>
/// Default implementation for building JWT claims.
/// </summary>
public sealed class ClaimsBuilder : IClaimsBuilder
{
    /// <inheritdoc />
    public IReadOnlyCollection<Claim> Build(
        CurrentUser user)
    {
        var claims = new List<Claim>();

        AddClaim(
            claims,
            JwtRegisteredClaimNames.Sub,
            user.UserId?.ToString());

        AddClaim(
            claims,
            ClaimTypes.NameIdentifier,
            user.UserId?.ToString());

        AddClaim(
            claims,
            ClaimTypes.Name,
            user.Username);

        AddClaim(
            claims,
            ClaimTypes.Email,
            user.Email);

        AddClaim(
            claims,
            OrionClaimTypes.TenantId,
            user.TenantId?.ToString());

        AddClaim(
            claims,
            OrionClaimTypes.BranchId,
            user.BranchId?.ToString());

        AddClaim(
            claims,
            OrionClaimTypes.Language,
            user.Language);

        AddClaim(
            claims,
            OrionClaimTypes.Timezone,
            user.Timezone);

        AddRoles(
            claims,
            user.Roles);

        AddPermissions(
            claims,
            user.Permissions);

        return claims;
    }

    private static void AddClaim(
        ICollection<Claim> claims,
        string type,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        claims.Add(
            new Claim(
                type,
                value));
    }

    private static void AddRoles(
        ICollection<Claim> claims,
        IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }
    }

    private static void AddPermissions(
        ICollection<Claim> claims,
        IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            claims.Add(
                new Claim(
                    OrionClaimTypes.Permission,
                    permission));
        }
    }
}

/// <summary>
/// Default JWT token generator.
/// </summary>
public sealed class JwtTokenGenerator(
    IOptions<JwtOptions> options,
    IClaimsBuilder claimsBuilder)
    : ITokenGenerator
{
    /// <inheritdoc />
    public TokenPair Generate(
        CurrentUser user)
    {
        var jwt = options.Value;

        var now =
            DateTimeOffset.UtcNow;

        var accessTokenExpiresAt =
            now.AddMinutes(
                jwt.AccessTokenMinutes);

        var signingKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    jwt.SigningKey));

        var credentials =
            new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

        var claims =
            claimsBuilder.Build(user);

        var token =
            new JwtSecurityToken(
                jwt.Issuer,
                jwt.Audience,
                claims,
                now.UtcDateTime,
                accessTokenExpiresAt.UtcDateTime,
                credentials);

        var refreshTokenBytes =
            RandomNumberGenerator.GetBytes(64);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        var refreshToken =
            Convert.ToBase64String(
                refreshTokenBytes);

        var refreshTokenExpiresAt =
            now.AddDays(
                jwt.RefreshTokenDays);

        return new TokenPair(
            accessToken,
            refreshToken,
            accessTokenExpiresAt,
            refreshTokenExpiresAt);
    }
}

/// <summary>
/// Default JWT token validator.
/// </summary>
public sealed class JwtTokenValidator(
    IOptions<JwtOptions> options)
    : ITokenValidator
{
    /// <inheritdoc />
    public ClaimsPrincipal Validate(
        string token)
    {
        var jwt = options.Value;

        var validationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,

                ValidateAudience = true,
                ValidAudience = jwt.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwt.SigningKey)),

                ValidateLifetime = true,

                ClockSkew =
                    TimeSpan.FromMinutes(1)
            };

        var tokenHandler =
            new JwtSecurityTokenHandler();

        return tokenHandler.ValidateToken(
            token,
            validationParameters,
            out _);
    }
}

/// <summary>
/// Claim type constants used by the Orion Framework.
/// </summary>
public static class OrionClaimTypes
{
    public const string TenantId = "tenant_id";

    public const string BranchId = "branch_id";

    public const string Permission = "permission";

    public const string Language = "language";

    public const string Timezone = "timezone";
}
