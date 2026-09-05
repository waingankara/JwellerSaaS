namespace JwellerSaaS.Contracts.Identity;

public sealed record LoginRequest(
    long TenantId,
    string Username,
    string Password);
