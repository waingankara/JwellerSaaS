using System.Security.Claims;
using Orion.Framework.Security;

namespace JwellerSaaS.Api.Security;

/// <summary>Builds the current user from ASP.NET Core claims.</summary>
public sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    /// <inheritdoc />
    public CurrentUser CurrentUser
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return CurrentUser.Anonymous;
            }

            var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = long.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : (long?)null;
            var roles = user.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return new CurrentUser(userId, user.Identity.Name, roles);
        }
    }
}
