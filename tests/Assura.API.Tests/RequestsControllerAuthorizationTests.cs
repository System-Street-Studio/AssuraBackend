using Assura.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Assura.API.Tests;

public class RequestsControllerAuthorizationTests
{
    [Fact]
    public void RequestsController_ShouldHaveClassLevelAuthorizeAttribute()
    {
        var controllerType = typeof(RequestsController);
        var authorize = controllerType
            .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .ToList();

        Assert.NotEmpty(authorize);
    }

    [Fact]
    public void ProcessRequest_ShouldAllowOnlyStorekeeperAdminProcurement()
    {
        var method = GetMethod(nameof(RequestsController.ProcessRequest));
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        AssertRoles(authorize!.Roles, "Storekeeper", "Admin", "Procurement");
    }

    [Fact]
    public void ReviewByDivisionHead_ShouldAllowOnlyDivisionHeadAndAdmin()
    {
        var method = GetMethod(nameof(RequestsController.ReviewByDivisionHead));
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        AssertRoles(authorize!.Roles, "DivisionHead", "Admin");
    }

    [Fact]
    public void ConfirmTemporaryAssignment_ShouldAllowOnlyStorekeeperAndAdmin()
    {
        var method = GetMethod(nameof(RequestsController.ConfirmTemporaryAssignment));
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        AssertRoles(authorize!.Roles, "Storekeeper", "Admin");
    }

    [Fact]
    public void GetSuggestedAssets_ShouldAllowOnlyStorekeeperAndAdmin()
    {
        var method = GetMethod(nameof(RequestsController.GetSuggestedAssets));
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        AssertRoles(authorize!.Roles, "Storekeeper", "Admin");
    }

    private static MethodInfo GetMethod(string methodName)
    {
        var method = typeof(RequestsController).GetMethod(methodName);
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
