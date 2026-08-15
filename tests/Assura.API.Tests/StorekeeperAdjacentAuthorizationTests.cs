using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the BUGS.md Storekeeper finding: "Four controllers touching
// storekeeper-adjacent data have no authorization at all."
public class StorekeeperAdjacentAuthorizationTests
{
    [Fact]
    public void CategoriesController_ShouldRequireAuthentication()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(CategoriesController)),
            "CategoriesController should inherit BaseApiController so all actions require authentication.");
    }

    [Theory]
    [InlineData(nameof(CategoriesController.CreateCategory))]
    [InlineData(nameof(CategoriesController.UpdateCategory))]
    [InlineData(nameof(CategoriesController.DeleteCategory))]
    public void CategoriesController_WriteActions_ShouldAllowOnlyAdminSystemAdminAndStorekeeper(string methodName)
    {
        // SystemAdmin's Master Data page also creates/edits/deletes categories via the
        // same CategoryService the Storekeeper's inventory feature uses (see
        // features/system-admin/pages/master-data/master-data.component.ts), so it must
        // stay in the allowed set alongside Admin and Storekeeper.
        var method = GetMethod<CategoriesController>(methodName);
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        AssertRoles(authorize!.Roles, Roles.Admin, Roles.SystemAdmin, Roles.Storekeeper);
    }

    [Fact]
    public void AssetSpecificationsController_ShouldRequireAuthentication()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(AssetSpecificationsController)),
            "AssetSpecificationsController should inherit BaseApiController so all actions require authentication.");
    }

    [Fact]
    public void ReceiptsController_ShouldAllowOnlyAccountantAndAdmin()
    {
        var controllerType = typeof(ReceiptsController);
        var authorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        AssertRoles(authorize.First().Roles, Roles.Accountant, Roles.Admin);
    }

    [Fact]
    public void QueueItemsController_ShouldAllowOnlySuperintendentAndAdmin()
    {
        var controllerType = typeof(QueueItemsController);
        var authorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        AssertRoles(authorize.First().Roles, Roles.Superintendent, Roles.Admin);
    }

    // Covers the BUGS.md Superintendent finding: "DiscardedNotesController has no
    // authorization at all — open to unauthenticated callers." Its only frontend
    // consumer is the Superintendent-gated discarded-notes page (shell.routes.ts
    // restricts /superintendent to Superintendent,Admin), matching QueueItemsController.
    [Fact]
    public void DiscardedNotesController_ShouldRequireAuthentication()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(DiscardedNotesController)),
            "DiscardedNotesController should inherit BaseApiController so all actions require authentication.");
    }

    [Fact]
    public void DiscardedNotesController_ShouldAllowOnlySuperintendentAndAdmin()
    {
        var controllerType = typeof(DiscardedNotesController);
        var authorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        AssertRoles(authorize.First().Roles, Roles.Superintendent, Roles.Admin);
    }

    private static MethodInfo GetMethod<TController>(string methodName)
    {
        var method = typeof(TController).GetMethod(methodName);
        Assert.NotNull(method);
        return method!;
    }

    private static void AssertRoles(string? roles, params string[] expectedRoles)
    {
        Assert.False(string.IsNullOrWhiteSpace(roles));

        var actual = roles!
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x)
            .ToArray();

        var expected = expectedRoles
            .OrderBy(x => x)
            .ToArray();

        Assert.Equal(expected, actual);
    }
}
