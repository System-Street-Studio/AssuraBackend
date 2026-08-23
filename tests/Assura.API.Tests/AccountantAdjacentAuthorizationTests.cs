using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the BUGS.md Accountant finding: "Four controllers touching accountant-adjacent
// data have no authorization at all" — AccPendingItemsController, AccDiscardedItemsController,
// AccDiscardNotesController, and LostItemsController were reachable by unauthenticated
// callers, including the destructive discard-confirm endpoint on AccPendingItemsController.
public class AccountantAdjacentAuthorizationTests
{
    [Fact]
    public void AccPendingItemsController_ShouldRequireAuthentication()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(AccPendingItemsController)),
            "AccPendingItemsController should inherit BaseApiController so all actions require authentication.");
    }

    [Fact]
    public void AccPendingItemsController_ShouldAllowOnlyAccountantAndAdmin()
    {
        var controllerType = typeof(AccPendingItemsController);
        var authorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        AssertRoles(authorize.First().Roles, Roles.Accountant, Roles.Admin);
    }

    [Fact]
    public void AccDiscardedItemsController_ShouldRequireAuthentication()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(AccDiscardedItemsController)),
            "AccDiscardedItemsController should inherit BaseApiController so all actions require authentication.");
    }

    [Fact]
    public void AccDiscardedItemsController_ShouldAllowOnlyAccountantAndAdmin()
    {
        var controllerType = typeof(AccDiscardedItemsController);
        var authorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        AssertRoles(authorize.First().Roles, Roles.Accountant, Roles.Admin);
    }

    [Fact]
    public void AccDiscardNotesController_ShouldRequireAuthentication()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(AccDiscardNotesController)),
            "AccDiscardNotesController should inherit BaseApiController so all actions require authentication.");
    }

    [Fact]
    public void AccDiscardNotesController_ShouldAllowOnlyAccountantAndAdmin()
    {
        var controllerType = typeof(AccDiscardNotesController);
        var authorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        AssertRoles(authorize.First().Roles, Roles.Accountant, Roles.Admin);
    }

    [Fact]
    public void LostItemsController_ShouldRequireAuthentication()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(LostItemsController)),
            "LostItemsController should inherit BaseApiController so all actions require authentication.");
    }

    // LostItems triage ownership belongs to Superintendent (mirrors DiscardedNotesController),
    // but Accountant keeps read access too — the frontend already has a live, working
    // Accountant "Lose" page (acc-lose) reading this list, so Accountant isn't dropped here.
    // Reporting a lost asset is open to Employee/Storekeeper too, but read/triage is not.
    [Fact]
    public void LostItemsController_GetAll_ShouldAllowSuperintendentAccountantAndAdmin()
    {
        var method = typeof(LostItemsController).GetMethod(nameof(LostItemsController.GetAll))!;
        var authorize = method.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        AssertRoles(authorize.First().Roles, Roles.Superintendent, Roles.Accountant, Roles.Admin);
    }

    [Fact]
    public void LostItemsController_Create_ShouldAllowReportingRoles()
    {
        var method = typeof(LostItemsController).GetMethod(nameof(LostItemsController.Create))!;
        var authorize = method.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        AssertRoles(authorize.First().Roles, Roles.Employee, Roles.Storekeeper, Roles.Superintendent, Roles.Admin);
    }

    [Fact]
    public void LostItemsController_UpdateStatus_ShouldAllowOnlySuperintendentAndAdmin()
    {
        var method = typeof(LostItemsController).GetMethod(nameof(LostItemsController.UpdateStatus))!;
        var authorize = method.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        AssertRoles(authorize.First().Roles, Roles.Superintendent, Roles.Admin);
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
