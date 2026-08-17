using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the BUGS.md Admin finding: "SystemAdminController has no [Authorize(Roles=...)] at
// all" — it only inherited the bare [Authorize] from BaseApiController (any authenticated
// user), so any logged-in Employee/Storekeeper/etc. could list all users, toggle-lock any
// account, download the full DB SQL backup, view error/security logs, and reset any user's
// password. Fixed by scoping the controller to Admin/SystemAdmin only, matching the pattern
// already used by CategoriesController.
public class SystemAdminControllerAuthorizationTests
{
    [Fact]
    public void SystemAdminController_ShouldBeRestrictedToAdminAndSystemAdminRoles()
    {
        var controllerType = typeof(SystemAdminController);
        var authorize = controllerType.GetCustomAttribute<AuthorizeAttribute>(inherit: false);

        Assert.NotNull(authorize);
        Assert.NotNull(authorize!.Roles);

        var roles = authorize.Roles!
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x)
            .ToArray();
        var expected = new[] { Roles.Admin, Roles.SystemAdmin }.OrderBy(x => x).ToArray();

        Assert.Equal(expected, roles);
    }

    [Fact]
    public void SystemAdminController_ShouldNotBeReachableByAnyAuthenticatedRole()
    {
        // Guards against a regression back to the bare [Authorize] (no Roles=) that only
        // BaseApiController provides — that would let every role in, not just Admin/SystemAdmin.
        var controllerType = typeof(SystemAdminController);
        var authorize = controllerType.GetCustomAttribute<AuthorizeAttribute>(inherit: false);

        Assert.NotNull(authorize);
        Assert.False(string.IsNullOrWhiteSpace(authorize!.Roles),
            "SystemAdminController must restrict Roles explicitly, not rely on the bare [Authorize] from BaseApiController.");
    }
}
