using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the BUGS.md Storekeeper finding: "GRN/GIN/TIN inventory documentation
// is entirely missing." GRNsController is the new backend piece of that feature.
public class GRNsControllerAuthorizationTests
{
    [Fact]
    public void GRNsController_ShouldRequireAuthentication()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(GRNsController)),
            "GRNsController should inherit BaseApiController so all actions require authentication.");
    }

    [Fact]
    public void CreateGRN_ShouldAllowOnlyAdminAndStorekeeper()
    {
        var method = typeof(GRNsController).GetMethod(nameof(GRNsController.CreateGRN));
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
