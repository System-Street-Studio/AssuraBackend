using Assura.API.Controllers;
using Assura.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

public class AuthorizationTests
{
    [Fact]
    public void AdminController_ShouldBeRestrictedToAdminRole()
    {
        var controllerType = typeof(AdminController);
        var authorizeAttrs = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true);

        Assert.NotEmpty(authorizeAttrs);
        Assert.Contains(authorizeAttrs, a => a.Roles == Roles.Admin);
    }

    [Fact]
    public void PurchasingOrdersController_ShouldHaveAuthorizeAttribute()
    {
        var controllerType = typeof(PurchasingOrdersController);
        var authorizeAttrs = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true);

        Assert.NotEmpty(authorizeAttrs);
    }

    [Fact]
    public void DashboardController_ShouldHaveAuthorizeAttribute()
    {
        var controllerType = typeof(DashboardController);
        var authorizeAttrs = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true);

        Assert.NotEmpty(authorizeAttrs);
    }

    [Fact]
    public void UserController_ShouldHaveAuthorizeAttribute()
    {
        var controllerType = typeof(UserController);
        var authorizeAttrs = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true);

        Assert.NotEmpty(authorizeAttrs);
    }
}
