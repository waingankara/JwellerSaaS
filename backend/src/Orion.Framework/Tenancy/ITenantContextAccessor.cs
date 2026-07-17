namespace Orion.Framework.Tenancy;

/// <summary>Provides access to the current tenant context.</summary>
public interface ITenantContextAccessor
{
    /// <summary>Gets the tenant context for the current request.</summary>
    TenantContext TenantContext { get; }
}
