using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers a bug found by the test-workflow simulation: PurchasingOrdersController had
// only the plain [Authorize] attribute (any authenticated user) on every action, so a
// live-tested Employee account could successfully create a purchasing order.
//
// The fix is per-action, not class-level: GetPurchasingOrders (the bare list) must stay
// open to any authenticated user, because Storekeeper legitimately calls it to populate
// the purchasing-order picker when recording a GRN (grn.service.ts's
// getPurchasingOrderOptions()) — restricting the whole controller to Procurement/Admin,
// as an earlier version of this fix did, broke that. Only the actions that create or
// mutate a purchasing order (create, update status, complete) plus the
// Procurement-specific dashboards (pending-requests, stats) are restricted.
public class PurchasingOrdersControllerAuthorizationTests
{
    [Fact]
    public void PurchasingOrdersController_ShouldInheritBaseApiController()
    {
        Assert.True(typeof(BaseApiController).IsAssignableFrom(typeof(PurchasingOrdersController)),
            "PurchasingOrdersController should inherit BaseApiController so all actions require authentication.");
    }

    [Fact]
    public void GetPurchasingOrders_ShouldStayOpenToAnyAuthenticatedUser()
    {
        var method = typeof(PurchasingOrdersController).GetMethod(nameof(PurchasingOrdersController.GetPurchasingOrders));
        Assert.NotNull(method);
        var authorize = method!.GetCustomAttribute<AuthorizeAttribute>();

        Assert.Null(authorize);
    }

    [Theory]
    [InlineData(nameof(PurchasingOrdersController.CreatePurchasingOrder))]
    [InlineData(nameof(PurchasingOrdersController.UpdateStatus))]
    [InlineData(nameof(PurchasingOrdersController.CompleteOrder))]
    [InlineData(nameof(PurchasingOrdersController.GetPendingRequests))]
    [InlineData(nameof(PurchasingOrdersController.GetProcurementStats))]
    public void MutatingAndProcurementOnlyActions_ShouldAllowOnlyProcurementAndAdmin(string methodName)
    {
        var method = typeof(PurchasingOrdersController).GetMethod(methodName);
        Assert.NotNull(method);
        var authorize = method!.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.False(string.IsNullOrEmpty(authorize!.Roles), $"{methodName} must restrict roles.");

        var roles = authorize.Roles!
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .OrderBy(x => x)
            .ToArray();
        var expected = new[] { Roles.Procurement, Roles.Admin }.OrderBy(x => x).ToArray();

        Assert.Equal(expected, roles);
    }
}
