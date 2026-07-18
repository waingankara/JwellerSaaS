using System.Security.Claims;
using Orion.Framework.Authentication;
using Orion.Framework.Security;

namespace JwellerSaaS.Api.Security;

public sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public CurrentUser CurrentUser
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return CurrentUser.Anonymous;
            static long? LongClaim(ClaimsPrincipal principal, string type) => long.TryParse(principal.FindFirstValue(type), out var value) ? value : null;
            var roles = user.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var permissions = user.FindAll(OrionClaimTypes.Permission).Select(claim => claim.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return new CurrentUser(LongClaim(user, ClaimTypes.NameIdentifier), user.Identity.Name, user.FindFirstValue(ClaimTypes.Email), LongClaim(user, OrionClaimTypes.TenantId), LongClaim(user, OrionClaimTypes.BranchId), roles, permissions, user.FindFirstValue(OrionClaimTypes.Language), user.FindFirstValue(OrionClaimTypes.Timezone));
        }
    }
}
