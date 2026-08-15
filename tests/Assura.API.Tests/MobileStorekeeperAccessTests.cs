using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

// Covers the BUGS.md Storekeeper finding: "Mobile app locks Storekeeper out
// entirely." The mobile app's only authenticated screen calls
// GET /api/Admin/dashboard-stats, which must accept Storekeeper accounts once
// the mobile login gate (auth_service.dart) is opened up to them.
public class MobileStorekeeperAccessTests
{
    [Fact]
    public void AdminController_DashboardStats_ShouldAllowStorekeeper()
    {
        var controllerType = typeof(AdminController);
        var authorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToList();

        Assert.NotEmpty(authorize);
        var roles = authorize.First().Roles;
        Assert.False(string.IsNullOrWhiteSpace(roles));

        var actual = roles!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Contains(Roles.Storekeeper, actual);
    }
}
