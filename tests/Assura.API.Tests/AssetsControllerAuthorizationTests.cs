using Assura.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the BUGS.md Employee finding: "Missing role restriction on Assets CRUD —
// Employee can create/edit/delete arbitrary assets." CreateAsset/UpdateAsset/DeleteAsset/
// PatchAssetStatus/CheckinAsset previously had no role restriction beyond the class-level
// [Authorize], so any authenticated role (including Employee) could mutate any asset.
public class AssetsControllerAuthorizationTests
{
    [Theory]
    [InlineData(nameof(AssetsController.CreateAsset))]
    [InlineData(nameof(AssetsController.UpdateAsset))]
    [InlineData(nameof(AssetsController.DeleteAsset))]
    [InlineData(nameof(AssetsController.PatchAssetStatus))]
    [InlineData(nameof(AssetsController.CheckinAsset))]
    public void MutatingEndpoint_ShouldAllowOnlyAdminAndStorekeeper(string methodName)
    {
        var method = typeof(AssetsController).GetMethod(methodName);
        Assert.NotNull(method);

        var authorize = method!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorize);

        var actual = authorize!.Roles!
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x)
            .ToArray();

        Assert.Equal(new[] { "Admin", "Storekeeper" }, actual);
    }
}
