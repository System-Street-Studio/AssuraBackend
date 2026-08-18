using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// The mobile app was previously opened up to Storekeeper accounts (see the old
// "Mobile app locks Storekeeper out entirely" fix), which required
// GET /api/Admin/dashboard-stats to accept Storekeeper too. Per explicit product
// clarification, the mobile app is Admin-only — its entire duty is QR-scanning and
// verifying assets — so that mobile-login change was reverted and this endpoint's
// Storekeeper access is no longer needed either.
public class MobileStorekeeperAccessTests
{
    [Fact]
    public void AdminController_DashboardStats_ShouldNotAllowStorekeeper()
    {
        var controllerType = typeof(AdminController);
        var authorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        var roles = authorize.First().Roles;
        Assert.False(string.IsNullOrWhiteSpace(roles));

        var actual = roles!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.DoesNotContain(Roles.Storekeeper, actual);
        Assert.Contains(Roles.Admin, actual);
    }
}
