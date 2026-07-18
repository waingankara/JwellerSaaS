using JwellerSaaS.Domain.Common;

namespace JwellerSaaS.Domain.Tenancy;

public sealed class Branch : BaseAuditEntity
{
    public long TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Timezone { get; set; } = "UTC";
    public string Country { get; set; } = "US";
    public bool IsDefault { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;
}
