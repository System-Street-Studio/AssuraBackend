using Assura.Application.Features.SystemAdmin.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests;

// Covers two BUGS.md Admin findings:
// 1. "Password reset silently sets a hardcoded default password with no notification" —
//    ResetUserPasswordCommand used to reset every user to the literal "Password@123" and
//    never told the affected user anything happened.
// 2. "Admin-driven user-state changes are entirely silent" — ToggleUserLockCommand flips
//    IsLocked with no notification either.
public class SystemAdminModuleTests
{
    [Fact]
    public async Task ResetUserPasswordCommand_GeneratesRandomPasswordNotHardcodedDefault()
    {
        using var db = CreateContext();

        var caller = new User { Id = 1, Username = "admin1", Email = "admin1@assura.test", Role = UserRole.SystemAdmin };
        var user = new User
        {
            Id = 40,
            Username = "target_user",
            FirstName = "Target",
            LastName = "User",
            Email = "target@assura.test",
            PasswordHash = "original-hash",
            Role = UserRole.Employee
        };
        db.Users.AddRange(caller, user);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        var result = await handler.Handle(new ResetUserPasswordCommand(user.Id, caller.Id), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.TemporaryPassword);
        Assert.NotEqual("Password@123", result.TemporaryPassword);

        var updated = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.NotEqual("original-hash", updated.PasswordHash);
        Assert.False(BCrypt.Net.BCrypt.Verify("Password@123", updated.PasswordHash),
            "Reset password must not fall back to the old hardcoded default.");
        Assert.True(BCrypt.Net.BCrypt.Verify(result.TemporaryPassword, updated.PasswordHash),
            "The returned temporary password must actually match the new hash.");
    }

    [Fact]
    public async Task ResetUserPasswordCommand_ProducesADifferentPasswordEachTime()
    {
        using var db = CreateContext();

        var caller = new User { Id = 2, Username = "admin2", Email = "admin2@assura.test", Role = UserRole.SystemAdmin };
        var user = new User { Id = 41, Username = "u2", Email = "u2@assura.test", PasswordHash = "x", Role = UserRole.Employee };
        db.Users.AddRange(caller, user);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        var first = await handler.Handle(new ResetUserPasswordCommand(user.Id, caller.Id), CancellationToken.None);
        var second = await handler.Handle(new ResetUserPasswordCommand(user.Id, caller.Id), CancellationToken.None);

        Assert.NotEqual(first.TemporaryPassword, second.TemporaryPassword);
    }

    [Fact]
    public async Task ResetUserPasswordCommand_CreatesNotificationForAffectedUser()
    {
        using var db = CreateContext();

        var caller = new User { Id = 3, Username = "admin3", Email = "admin3@assura.test", Role = UserRole.SystemAdmin };
        var user = new User { Id = 42, Username = "u3", Email = "u3@assura.test", PasswordHash = "x", Role = UserRole.Employee };
        db.Users.AddRange(caller, user);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        await handler.Handle(new ResetUserPasswordCommand(user.Id, caller.Id), CancellationToken.None);

        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == user.Id);
        Assert.NotNull(notification);
        Assert.DoesNotContain(notification!.Message, notification.Title); // sanity: fields are populated, not identical
    }

    [Fact]
    public async Task ResetUserPasswordCommand_StillRefusesToResetSysadmin()
    {
        using var db = CreateContext();

        var caller = new User { Id = 4, Username = "admin4", Email = "admin4@assura.test", Role = UserRole.SystemAdmin };
        var sysadmin = new User { Id = 43, Username = "sysadmin", Email = "sysadmin@assura.test", PasswordHash = "x", Role = UserRole.SystemAdmin };
        db.Users.AddRange(caller, sysadmin);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        var result = await handler.Handle(new ResetUserPasswordCommand(sysadmin.Id, caller.Id), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Null(result.TemporaryPassword);
    }

    [Fact]
    public async Task ToggleUserLockCommand_LockingCreatesNotification()
    {
        using var db = CreateContext();

        var caller = new User { Id = 5, Username = "admin5", Email = "admin5@assura.test", Role = UserRole.SystemAdmin };
        var user = new User { Id = 44, Username = "u4", Email = "u4@assura.test", PasswordHash = "x", IsLocked = false, Role = UserRole.Employee };
        db.Users.AddRange(caller, user);
        await db.SaveChangesAsync();

        var handler = new ToggleUserLockCommandHandler(db);
        var success = await handler.Handle(new ToggleUserLockCommand(user.Id, caller.Id), CancellationToken.None);

        Assert.True(success);
        var updated = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.True(updated.IsLocked);

        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == user.Id);
        Assert.NotNull(notification);
        Assert.Equal("Account Locked", notification!.Title);
    }

    [Fact]
    public async Task ToggleUserLockCommand_UnlockingCreatesDifferentNotification()
    {
        using var db = CreateContext();

        var caller = new User { Id = 6, Username = "admin6", Email = "admin6@assura.test", Role = UserRole.SystemAdmin };
        var user = new User { Id = 45, Username = "u5", Email = "u5@assura.test", PasswordHash = "x", IsLocked = true, Role = UserRole.Employee };
        db.Users.AddRange(caller, user);
        await db.SaveChangesAsync();

        var handler = new ToggleUserLockCommandHandler(db);
        await handler.Handle(new ToggleUserLockCommand(user.Id, caller.Id), CancellationToken.None);

        var updated = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.False(updated.IsLocked);

        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == user.Id);
        Assert.NotNull(notification);
        Assert.Equal("Account Unlocked", notification!.Title);
    }

    [Fact]
    public void ResetUserPasswordCommandValidator_RejectsInvalidUserId()
    {
        var validator = new ResetUserPasswordCommandValidator();
        var command = new ResetUserPasswordCommand(0, 10);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void ResetUserPasswordCommandValidator_RejectsInvalidCallerUserId()
    {
        var validator = new ResetUserPasswordCommandValidator();
        var command = new ResetUserPasswordCommand(10, 0);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CallerUserId);
    }

    [Fact]
    public async Task ResetUserPasswordCommand_PreventsSelfTargeting()
    {
        using var db = CreateContext();

        var admin = new User { Id = 50, Username = "self_admin", Email = "self@assura.test", Role = UserRole.SystemAdmin };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        var result = await handler.Handle(new ResetUserPasswordCommand(admin.Id, admin.Id), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Null(result.TemporaryPassword);
    }

    [Fact]
    public async Task ResetUserPasswordCommand_PreventsAdminOnAdmin()
    {
        using var db = CreateContext();

        var admin1 = new User { Id = 51, Username = "admin_a", Email = "admin_a@assura.test", Role = UserRole.SystemAdmin };
        var admin2 = new User { Id = 52, Username = "admin_b", Email = "admin_b@assura.test", Role = UserRole.Admin };
        db.Users.AddRange(admin1, admin2);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        var result = await handler.Handle(new ResetUserPasswordCommand(admin2.Id, admin1.Id), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Null(result.TemporaryPassword);
    }

    [Fact]
    public void ToggleUserLockCommandValidator_RejectsInvalidUserId()
    {
        var validator = new ToggleUserLockCommandValidator();
        var command = new ToggleUserLockCommand(-5, 10);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void ToggleUserLockCommandValidator_RejectsInvalidCallerUserId()
    {
        var validator = new ToggleUserLockCommandValidator();
        var command = new ToggleUserLockCommand(10, -1);
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CallerUserId);
    }

    [Fact]
    public async Task ToggleUserLockCommand_PreventsSelfTargeting()
    {
        using var db = CreateContext();

        var admin = new User { Id = 53, Username = "lock_self", Email = "lock_self@assura.test", Role = UserRole.SystemAdmin, IsLocked = false };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        var handler = new ToggleUserLockCommandHandler(db);
        var success = await handler.Handle(new ToggleUserLockCommand(admin.Id, admin.Id), CancellationToken.None);

        Assert.False(success);
        var unchanged = await db.Users.FirstAsync(u => u.Id == admin.Id);
        Assert.False(unchanged.IsLocked);
    }

    [Fact]
    public async Task ToggleUserLockCommand_PreventsAdminOnAdmin()
    {
        using var db = CreateContext();

        var systemAdmin = new User { Id = 54, Username = "sys_admin", Email = "sys@assura.test", Role = UserRole.SystemAdmin };
        var regularAdmin = new User { Id = 55, Username = "reg_admin", Email = "reg@assura.test", Role = UserRole.Admin, IsLocked = false };
        db.Users.AddRange(systemAdmin, regularAdmin);
        await db.SaveChangesAsync();

        var handler = new ToggleUserLockCommandHandler(db);
        var success = await handler.Handle(new ToggleUserLockCommand(regularAdmin.Id, systemAdmin.Id), CancellationToken.None);

        Assert.False(success);
        var unchanged = await db.Users.FirstAsync(u => u.Id == regularAdmin.Id);
        Assert.False(unchanged.IsLocked);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
