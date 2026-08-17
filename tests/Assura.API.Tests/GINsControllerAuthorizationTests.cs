using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the test-workflow finding: GIN had a domain entity and DB migrations but
// no controller, no commands/queries, and no frontend page at all — completely
// unreachable. GINsController is the new backend piece of that feature, built to
// mirror GRNsController exactly (read is open to any authenticated user, create is
// Storekeeper/Admin only).
public class GINsControllerAuthorizationTests
{
    [Fact]
    public void GINsController_ShouldRequireAuthentication()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(GINsController)),
            "GINsController should inherit BaseApiController so all actions require authentication.");
    }

    [Fact]
    public void CreateGIN_ShouldAllowOnlyAdminAndStorekeeper()
    {
        var method = typeof(GINsController).GetMethod(nameof(GINsController.CreateGIN));
        Assert.NotNull(method);
        var authorize = method!.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        var roles = authorize!.Roles!
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x)
            .ToArray();
        var expected = new[] { Roles.Admin, Roles.Storekeeper }.OrderBy(x => x).ToArray();

        Assert.Equal(expected, roles);
    }
}
