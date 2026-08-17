using Assura.Application.Common.Interfaces;
using Assura.Application.Features.SystemAdmin.Commands;
using Assura.Application.Features.Users.Commands.ForgotPassword;
using Assura.Application.Features.Users.Commands.ResetPassword;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Assura.Infrastructure.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Assura.Application.Tests;

public class ResetPasswordFixTests
{
    private static IdentityService CreateService(out TestApplicationDbContext db)
    {
        db = TestContextFactory.CreateContext();
        var jwtGenerator = new Mock<IJwtTokenGenerator>();
        return new IdentityService(db, jwtGenerator.Object);
    }

    private static User NewResettableUser(int id, string sessionId = "old-session-abc") => new()
    {
        Id = id,
        Username = $"reset_user_{id}",
        Email = $"reset{id}@assura.test",
        FirstName = "Reset",
        LastName = "User",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword1"),
        PasswordResetToken = "valid-token",
        ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(30),
        CurrentSessionId = sessionId,
        RefreshToken = "old-refresh-token",
        RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
        IsActive = true,
        IsLocked = false,
        EmploymentStatus = "Assigned",
        Role = UserRole.Employee
    };

    [Fact]
    public async Task ResetPasswordAsync_Success_InvalidatesSessionAndRefreshToken()
    {
        var service = CreateService(out var db);
        var user = NewResettableUser(200);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await service.ResetPasswordAsync(user.Email, "valid-token", "NewPassword1");

        Assert.True(result);
        Assert.NotNull(user.CurrentSessionId);
        Assert.NotEqual("old-session-abc", user.CurrentSessionId);
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiryTime);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPassword1", user.PasswordHash));
    }

    [Fact]
    public async Task ResetPasswordAsync_LockedAccount_Fails()
    {
        var service = CreateService(out var db);
        var user = NewResettableUser(201);
        user.IsLocked = true;
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await service.ResetPasswordAsync(user.Email, "valid-token", "NewPassword1");

        Assert.False(result);
        Assert.True(BCrypt.Net.BCrypt.Verify("OldPassword1", user.PasswordHash));
    }

    [Fact]
    public async Task ResetPasswordAsync_InactiveAccount_Fails()
    {
        var service = CreateService(out var db);
        var user = NewResettableUser(202);
        user.IsActive = false;
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await service.ResetPasswordAsync(user.Email, "valid-token", "NewPassword1");

        Assert.False(result);
    }

    [Fact]
    public async Task ResetUserPasswordCommand_Success_InvalidatesSessionAndRefreshToken()
    {
        var db = TestContextFactory.CreateContext();
        var caller = new User { Id = 300, Username = "admin300", Email = "admin300@assura.test", PasswordHash = "x", Role = UserRole.Admin };
        var target = new User
        {
            Id = 301,
            Username = "target301",
            Email = "target301@assura.test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword1"),
            CurrentSessionId = "old-session-xyz",
            RefreshToken = "old-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
            Role = UserRole.Employee
        };
        db.Users.AddRange(caller, target);
        await db.SaveChangesAsync();

        var handler = new ResetUserPasswordCommandHandler(db);
        var result = await handler.Handle(new ResetUserPasswordCommand(target.Id, caller.Id), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(target.CurrentSessionId);
        Assert.NotEqual("old-session-xyz", target.CurrentSessionId);
        Assert.Null(target.RefreshToken);
        Assert.Null(target.RefreshTokenExpiryTime);
    }

    [Fact]
    public void ForgotPasswordCommandValidator_RejectsEmptyEmail()
    {
        var validator = new ForgotPasswordCommandValidator();
        var result = validator.Validate(new ForgotPasswordCommand(""));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ForgotPasswordCommandValidator_RejectsMalformedEmail()
    {
        var validator = new ForgotPasswordCommandValidator();
        var result = validator.Validate(new ForgotPasswordCommand("not-an-email"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ForgotPasswordCommandValidator_AcceptsValidEmail()
    {
        var validator = new ForgotPasswordCommandValidator();
        var result = validator.Validate(new ForgotPasswordCommand("valid@assura.test"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ResetPasswordCommandValidator_RejectsShortPassword()
    {
        var validator = new ResetPasswordCommandValidator();
        var result = validator.Validate(new ResetPasswordCommand("user@assura.test", "some-token", "abc"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ResetPasswordCommandValidator_AcceptsSixCharacterPassword()
    {
        // Must stay in sync with the pre-existing Angular/Flutter client minLength(6) -
        // a stricter backend policy than the clients already enforce would silently
        // start rejecting previously-successful resets from either client.
        var validator = new ResetPasswordCommandValidator();
        var result = validator.Validate(new ResetPasswordCommand("user@assura.test", "some-token", "abcdef"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ResetPasswordCommandValidator_RejectsEmptyToken()
    {
        var validator = new ResetPasswordCommandValidator();
        var result = validator.Validate(new ResetPasswordCommand("user@assura.test", "", "abcdef"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ForgotPasswordCommandHandler_BuildsResetLink_FromConfiguredFrontendBaseUrl_NotHardcodedLocalhost()
    {
        var identifyServices = new Mock<IIdentifyServices>();
        identifyServices.Setup(s => s.GeneratePasswordResetTokenAsync("user@assura.test"))
            .ReturnsAsync("the-token");

        string? capturedBody = null;
        var emailService = new Mock<IEmailService>();
        emailService.Setup(s => s.SendEmailAsync("user@assura.test", It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, body) => capturedBody = body)
            .Returns(Task.CompletedTask);

        var appUrlsService = new Mock<IAppUrlsService>();
        appUrlsService.Setup(s => s.FrontendBaseUrl).Returns("https://assura.example.com");

        var handler = new ForgotPasswordCommandHandler(
            identifyServices.Object,
            emailService.Object,
            appUrlsService.Object,
            new Mock<ILogger<ForgotPasswordCommandHandler>>().Object);

        await handler.Handle(new ForgotPasswordCommand("user@assura.test"), CancellationToken.None);

        Assert.NotNull(capturedBody);
        Assert.Contains("https://assura.example.com/auth/reset-password?token=the-token", capturedBody);
        Assert.DoesNotContain("localhost:4200", capturedBody);
    }
}
