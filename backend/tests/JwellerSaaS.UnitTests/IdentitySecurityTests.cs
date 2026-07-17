using Microsoft.Extensions.Options;
using Orion.Framework.Authentication;
using Orion.Framework.Authorization;
using Orion.Framework.Identity;
using Orion.Framework.Options;
using Orion.Framework.Security;
using Xunit;

namespace JwellerSaaS.UnitTests;

public sealed class IdentitySecurityTests
{
    private static readonly CurrentUser User = new(7, "admin", "admin@example.com", 1, 2, new HashSet<string> { "ADMINISTRATOR" }, new HashSet<string> { PermissionConstants.Administrator }, "en", "UTC");

    [Fact]
    public void JwtRoundTripMapsClaims()
    {
        var options = Options.Create(new JwtOptions { Issuer = "tests", Audience = "tests", SigningKey = "test-signing-key-with-at-least-32chars" });
        var pair = new JwtTokenGenerator(options, new ClaimsBuilder()).Generate(User);
        var principal = new JwtTokenValidator(options).Validate(pair.AccessToken);
        Assert.Equal("7", principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void PasswordHasherVerifiesAndRejectsWrongPassword()
    {
        IPasswordHasher hasher = new MicrosoftPasswordHasher();
        var hash = hasher.HashPassword("Str0ngPassword!");
        Assert.True(hasher.VerifyPassword(hash, "Str0ngPassword!"));
        Assert.False(hasher.VerifyPassword(hash, "wrong"));
    }

    [Fact]
    public async Task PermissionServiceAllowsAdministratorPermission()
    {
        var allowed = await new PermissionService().HasPermissionAsync(User, PermissionConstants.TenantWrite, CancellationToken.None);
        Assert.True(allowed);
    }

    [Fact]
    public void CurrentUserContainsTenantBranchAndLocalization()
    {
        Assert.Equal(1, User.TenantId);
        Assert.Equal(2, User.BranchId);
        Assert.Contains("ADMINISTRATOR", User.Roles);
        Assert.Equal("UTC", User.Timezone);
    }
}
