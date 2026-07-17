namespace Orion.Framework.Tenancy;

/// <summary>Represents the current tenant scope.</summary>
public sealed record TenantContext(long? TenantId, bool IsGlobalScope)
{
    /// <summary>Gets a global tenant context.</summary>
    public static TenantContext Global { get; } = new(null, true);
}
