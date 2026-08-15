using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the BUGS.md Employee finding: "Minor info disclosure — assignable-users list
// endpoint has no role restriction." GET /api/users/assignable-users is only ever
// called by the Storekeeper/Admin checkout flow (checkout.service.ts), so any other
// authenticated role (including Employee) could enumerate assignable users.
public class UserControllerAuthorizationTests
{
    [Fact]
    public void GetAssignableUsers_ShouldAllowOnlyAdminAndStorekeeper()
    {
        var method = typeof(UserController).GetMethod(nameof(UserController.GetAssignableUsers));
        Assert.NotNull(method);

        var authorize = method!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorize);

        var actual = authorize!.Roles!
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x)
            .ToArray();

        Assert.Equal(new[] { Roles.Admin, Roles.Storekeeper }.OrderBy(x => x).ToArray(), actual);
    }
}
