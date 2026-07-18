using JwellerSaaS.Domain.Common;

namespace JwellerSaaS.Domain.Tenancy;

public sealed class Tenant : BaseAuditEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? DefaultBranchId { get; set; }
    public string Timezone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
    public string Country { get; set; } = "US";
    public string? LogoUrl { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Trial;
}
