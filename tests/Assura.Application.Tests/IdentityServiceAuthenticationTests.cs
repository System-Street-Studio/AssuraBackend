using Assura.Application.Common.Interfaces;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Assura.Infrastructure.Identity;
using Moq;

namespace Assura.Application.Tests;

public class IdentityServiceAuthenticationTests
{
    private static IdentityService CreateService(out TestApplicationDbContext db)
    {
        db = CreateContext();
        var jwtGenerator = new Mock<IJwtTokenGenerator>();
        jwtGenerator.Setup(g => g.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");
        return new IdentityService(db, jwtGenerator.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_RejectedUser_ThrowsUnauthorizedWithRejectionMessage()
    {
        var service = CreateService(out var db);

        var user = new User
        {
            Id = 50,
            Username = "rejected_user",
            Email = "rejected@assura.test",
            FirstName = "Rejected",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = false,
            EmploymentStatus = "Rejected",
            Role = null
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.AuthenticateAsync("rejected_user", "Password123!"));

        Assert.Contains("rejected", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuthenticateAsync_InactiveNonRejectedUser_ThrowsUnauthorizedWithGenericInactiveMessage()
    {
        var service = CreateService(out var db);

        var user = new User
        {
            Id = 51,
            Username = "deactivated_user",
            Email = "deactivated@assura.test",
            FirstName = "Deactivated",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = false,
            EmploymentStatus = "Assigned",
            Role = UserRole.Employee
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.AuthenticateAsync("deactivated_user", "Password123!"));

        Assert.DoesNotContain("rejected", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("inactive", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AuthenticateAsync_ActivePendingUser_StillSucceeds()
    {
        var service = CreateService(out var db);

        var user = new User
        {
            Id = 52,
            Username = "pending_user",
            Email = "pending@assura.test",
            FirstName = "Pending",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = true,
            EmploymentStatus = "PendingAssignment",
            Role = null
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await service.AuthenticateAsync("pending_user", "Password123!");

        Assert.NotNull(result);
        Assert.Equal("fake-jwt-token", result!.Token);
    }

    [Fact]
    public async Task AuthenticateAsync_ActiveAssignedUser_StillSucceeds()
    {
        var service = CreateService(out var db);

        var user = new User
        {
            Id = 53,
            Username = "active_user",
            Email = "active@assura.test",
            FirstName = "Active",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            IsActive = true,
            EmploymentStatus = "Assigned",
            Role = UserRole.Employee
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await service.AuthenticateAsync("active_user", "Password123!");

        Assert.NotNull(result);
        Assert.Equal("fake-jwt-token", result!.Token);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
