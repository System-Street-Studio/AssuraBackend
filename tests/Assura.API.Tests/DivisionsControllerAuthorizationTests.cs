using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the BUGS.md Admin finding: "DivisionsController has no authorization at all" —
// it derived from bare ControllerBase (not BaseApiController) with no [Authorize] anywhere,
// so anyone unauthenticated could create/update/delete divisions. Reads stay open to any
// authenticated user (dropdowns across many roles depend on them); writes are Admin/SystemAdmin.
public class DivisionsControllerAuthorizationTests
{
    [Fact]
    public void DivisionsController_ShouldRequireAuthentication()
    {
        var authorizeAttrs = typeof(DivisionsController).GetCustomAttributes<AuthorizeAttribute>(inherit: false);
        Assert.Contains(authorizeAttrs, a => string.IsNullOrEmpty(a.Roles));
    }

    [Theory]
    [InlineData(nameof(DivisionsController.CreateDivision))]
    [InlineData(nameof(DivisionsController.UpdateDivision))]
    [InlineData(nameof(DivisionsController.DeleteDivision))]
    public void MutatingActions_ShouldAllowOnlyAdminAndSystemAdmin(string methodName)
    {
        var method = typeof(DivisionsController).GetMethods()
            .First(m => m.Name == methodName && m.GetCustomAttribute<AuthorizeAttribute>() != null);
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        var roles = authorize!.Roles!
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x)
            .ToArray();
        var expected = new[] { Roles.Admin, Roles.SystemAdmin }.OrderBy(x => x).ToArray();

        Assert.Equal(expected, roles);
    }

    [Fact]
    public void GetDivisions_ShouldNotBeRoleRestricted()
    {
        var method = typeof(DivisionsController).GetMethod(nameof(DivisionsController.GetDivisions));
        Assert.NotNull(method);
        Assert.Null(method!.GetCustomAttribute<AuthorizeAttribute>());
    }
}
