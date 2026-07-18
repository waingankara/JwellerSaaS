namespace Orion.Framework.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
}

public sealed class TenantOptions
{
    public const string SectionName = "Tenant";
    public string HeaderName { get; set; } = "X-Tenant-Id";
    public bool Required { get; set; } = true;
}

public sealed class SecurityOptions
{
    public const string SectionName = "Security";
    public int PasswordHistoryLimit { get; set; } = 5;
    public int MinimumPasswordLength { get; set; } = 8;
}
