using Assura.Application.Features.SystemAdmin.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
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

        var user = new User
        {
            Id = 40,
            Username = "target_user",
            FirstName = "Target",
            LastName = "User",
            Email = "target@assura.test",
            PasswordHash = "original-hash"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        var result = await handler.Handle(new ResetUserPasswordCommand(user.Id), CancellationToken.None);

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

        var user = new User { Id = 41, Username = "u2", Email = "u2@assura.test", PasswordHash = "x" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        var first = await handler.Handle(new ResetUserPasswordCommand(user.Id), CancellationToken.None);
        var second = await handler.Handle(new ResetUserPasswordCommand(user.Id), CancellationToken.None);

        Assert.NotEqual(first.TemporaryPassword, second.TemporaryPassword);
    }

    [Fact]
    public async Task ResetUserPasswordCommand_CreatesNotificationForAffectedUser()
    {
        using var db = CreateContext();

        var user = new User { Id = 42, Username = "u3", Email = "u3@assura.test", PasswordHash = "x" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        await handler.Handle(new ResetUserPasswordCommand(user.Id), CancellationToken.None);

        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == user.Id);
        Assert.NotNull(notification);
        Assert.DoesNotContain(notification!.Message, notification.Title); // sanity: fields are populated, not identical
    }

    [Fact]
    public async Task ResetUserPasswordCommand_StillRefusesToResetSysadmin()
    {
        using var db = CreateContext();

        var sysadmin = new User { Id = 43, Username = "sysadmin", Email = "sysadmin@assura.test", PasswordHash = "x" };
        db.Users.Add(sysadmin);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        var result = await handler.Handle(new ResetUserPasswordCommand(sysadmin.Id), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Null(result.TemporaryPassword);
    }

    [Fact]
    public async Task ToggleUserLockCommand_LockingCreatesNotification()
    {
        using var db = CreateContext();

        var user = new User { Id = 44, Username = "u4", Email = "u4@assura.test", PasswordHash = "x", IsLocked = false };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new ToggleUserLockCommandHandler(db);
        var success = await handler.Handle(new ToggleUserLockCommand(user.Id), CancellationToken.None);

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

        var user = new User { Id = 45, Username = "u5", Email = "u5@assura.test", PasswordHash = "x", IsLocked = true };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var handler = new ToggleUserLockCommandHandler(db);
        await handler.Handle(new ToggleUserLockCommand(user.Id), CancellationToken.None);

        var updated = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.False(updated.IsLocked);

        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == user.Id);
        Assert.NotNull(notification);
        Assert.Equal("Account Unlocked", notification!.Title);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
