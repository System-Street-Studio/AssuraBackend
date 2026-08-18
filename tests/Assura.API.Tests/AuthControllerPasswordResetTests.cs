using Assura.API.Controllers;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Users.Commands.ForgotPassword;
using Assura.Application.Features.Users.Commands.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Moq;

namespace Assura.API.Tests;

// Covers the BUGS.md Authentication finding: "No test coverage for the forgot/reset
// password feature" - no test files previously existed for AuthController's
// forgot/reset endpoints at all.
public class AuthControllerPasswordResetTests
{
    private static AuthController CreateController(Mock<IMediator> mediator) =>
        new(mediator.Object, Mock.Of<IIdentifyServices>());

    [Fact]
    public async Task ForgotPassword_AlwaysReturnsGenericOkMessage_RegardlessOfCommandResult()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var controller = CreateController(mediator);
        var result = await controller.ForgotPassword(new ForgotPasswordCommand("someone@assura.test"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = ok.Value!.ToString();
        Assert.Contains("If an account exists", body);
        // The token must never appear in the response body - this exact regression was
        // a fixed Critical-priority account-takeover bug (see BUGS.md Authentication section).
        Assert.DoesNotContain("Token", body);
    }

    [Fact]
    public async Task ResetPassword_Success_ReturnsOk()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController(mediator);
        var result = await controller.ResetPassword(new ResetPasswordCommand("user@assura.test", "token", "NewPassword1"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Failure_ReturnsBadRequest()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = CreateController(mediator);
        var result = await controller.ResetPassword(new ResetPasswordCommand("user@assura.test", "bad-token", "NewPassword1"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData(nameof(AuthController.ForgotPassword))]
    [InlineData(nameof(AuthController.ResetPassword))]
    public void PasswordResetEndpoints_HaveRateLimitingApplied(string actionName)
    {
        var method = typeof(AuthController).GetMethod(actionName);
        Assert.NotNull(method);

        var rateLimit = method!.GetCustomAttributes(typeof(EnableRateLimitingAttribute), false);
        Assert.Single(rateLimit);
        Assert.Equal("PasswordReset", ((EnableRateLimitingAttribute)rateLimit[0]).PolicyName);
    }
}
