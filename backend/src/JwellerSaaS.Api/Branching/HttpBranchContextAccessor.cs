using Orion.Framework.Authentication;
using Orion.Framework.Branching;

namespace JwellerSaaS.Api.Branching;

public sealed class HttpBranchContextAccessor(IHttpContextAccessor httpContextAccessor) : IBranchContextAccessor
{
    public BranchContext Current
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst(OrionClaimTypes.BranchId)?.Value;
            return long.TryParse(value, out var branchId) ? new BranchContext(branchId) : new BranchContext(null);
        }
    }
}
