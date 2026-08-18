using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the BUGS.md Admin finding: "SeedController has no authorization at all — fully
// public/unauthenticated endpoints", including one that resets the admin/sysadmin accounts
// to hardcoded default passwords and one that runs raw SQL updates. Restricted to
// Admin/SystemAdmin; a fresh deployment can still reach it because DbInitializer now
// bootstraps a default Admin account if none exists.
public class SeedControllerAuthorizationTests
{
    [Fact]
    public void SeedController_ShouldBeRestrictedToAdminAndSystemAdminRoles()
    {
        var authorize = typeof(SeedController).GetCustomAttribute<AuthorizeAttribute>(inherit: false);

        Assert.NotNull(authorize);
        Assert.NotNull(authorize!.Roles);

        var roles = authorize.Roles!
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x)
            .ToArray();
        var expected = new[] { Roles.Admin, Roles.SystemAdmin }.OrderBy(x => x).ToArray();

        Assert.Equal(expected, roles);
    }
}
