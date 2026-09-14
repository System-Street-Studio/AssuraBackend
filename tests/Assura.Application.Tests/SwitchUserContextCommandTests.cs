using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Users.Commands.SwitchContext;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Assura.Application.Tests;

public class SwitchUserContextCommandTests
{
    private readonly Mock<IJwtTokenGenerator> _mockJwtGenerator;

    public SwitchUserContextCommandTests()
    {
        _mockJwtGenerator = new Mock<IJwtTokenGenerator>();
        _mockJwtGenerator
            .Setup(g => g.GenerateToken(It.IsAny<User>(), It.IsAny<string?>(), It.IsAny<int?>()))
            .Returns("mock-token-switched");
    }

    [Fact]
    public async Task Handle_SwitchToEmployee_DoesNotOverwritePermanentRoleInDatabase()
    {
        using var context = TestContextFactory.CreateContext();

        var storekeeper = new User
        {
            Id = 101,
            Username = "test_storekeeper",
            Email = "storekeeper@assura.com",
            Role = UserRole.Storekeeper,
            DivisionId = 10,
            IsActive = true
        };
        context.Users.Add(storekeeper);
        await context.SaveChangesAsync();

        var handler = new SwitchUserContextCommandHandler(context, _mockJwtGenerator.Object);

        var result = await handler.Handle(new SwitchUserContextCommand
        {
            UserId = 101,
            Role = "Employee",
            DivisionId = 10
        }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("mock-token-switched", result.Token);
        Assert.Equal(10, result.DivisionId);
        Assert.Equal("Employee", result.Role);

        // Verify that in the database, the user's permanent role is NOT modified to Employee
        var userInDb = await context.Users
            .Include(u => u.DivisionRoles)
            .FirstAsync(u => u.Id == 101);

        Assert.Equal(UserRole.Storekeeper, userInDb.Role);

        // Verify that Storekeeper was preserved in DivisionRoles
        Assert.Contains(userInDb.DivisionRoles, dr => dr.Role == UserRole.Storekeeper && dr.DivisionId == 10);

        // Verify token generator received activeRole and activeDivisionId
        _mockJwtGenerator.Verify(g => g.GenerateToken(It.IsAny<User>(), "Employee", 10), Times.Once);
    }

    [Fact]
    public async Task Handle_SwitchBackToStorekeeper_SucceedsAfterSwitchingToEmployee()
    {
        using var context = TestContextFactory.CreateContext();

        var storekeeper = new User
        {
            Id = 102,
            Username = "test_storekeeper_2",
            Email = "storekeeper2@assura.com",
            Role = UserRole.Storekeeper,
            DivisionId = 10,
            IsActive = true
        };
        context.Users.Add(storekeeper);
        await context.SaveChangesAsync();

        var handler = new SwitchUserContextCommandHandler(context, _mockJwtGenerator.Object);

        // First switch to Employee
        var result1 = await handler.Handle(new SwitchUserContextCommand
        {
            UserId = 102,
            Role = "Employee",
            DivisionId = 10
        }, CancellationToken.None);
        Assert.True(result1.Success);

        // Now switch BACK to Storekeeper
        var result2 = await handler.Handle(new SwitchUserContextCommand
        {
            UserId = 102,
            Role = "Storekeeper",
            DivisionId = 10
        }, CancellationToken.None);

        Assert.True(result2.Success);
        Assert.Equal(10, result2.DivisionId);
        Assert.Equal("Storekeeper", result2.Role);

        var userInDb = await context.Users.FirstAsync(u => u.Id == 102);
        Assert.Equal(UserRole.Storekeeper, userInDb.Role);
    }

    [Fact]
    public async Task Handle_UnauthorizedRole_ReturnsFailure()
    {
        using var context = TestContextFactory.CreateContext();

        var storekeeper = new User
        {
            Id = 103,
            Username = "test_storekeeper_3",
            Email = "storekeeper3@assura.com",
            Role = UserRole.Storekeeper,
            DivisionId = 10,
            IsActive = true
        };
        context.Users.Add(storekeeper);
        await context.SaveChangesAsync();

        var handler = new SwitchUserContextCommandHandler(context, _mockJwtGenerator.Object);

        var result = await handler.Handle(new SwitchUserContextCommand
        {
            UserId = 103,
            Role = "Admin", // Not authorized to switch to Admin
            DivisionId = 10
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("not authorized", result.Error, StringComparison.OrdinalIgnoreCase);
    }
}
