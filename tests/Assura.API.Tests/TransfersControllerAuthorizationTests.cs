using System.Security.Claims;
using Assura.API.Controllers;
using Assura.Application.Features.Transfers.DTOs;
using Assura.Application.Features.Transfers.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Assura.API.Tests;

// Covers the BUGS.md Division Head finding: "IDOR: GET /api/transfers/counts takes
// userId from the query string with no role check" — any authenticated user could view
// any other user's dashboard counts by passing their id. The endpoint no longer
// accepts a userId parameter at all; it's always taken from the caller's JWT.
public class TransfersControllerAuthorizationTests
{
    [Fact]
    public async Task GetTransferCounts_UsesCallerIdFromJwt()
    {
        var mediatorMock = new Mock<IMediator>();
        GetTransferCountsQuery? captured = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTransferCountsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<TransferCountsDto>, CancellationToken>((q, _) => captured = (GetTransferCountsQuery)q)
            .ReturnsAsync(new TransferCountsDto());

        var controller = BuildController(mediatorMock, userId: 42, role: "DivisionHead");

        var result = await controller.GetTransferCounts();

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(captured);
        Assert.Equal(42, captured!.LoginUserId);
    }

    [Fact]
    public async Task GetTransferCounts_NoAuthenticatedCaller_ReturnsUnauthorized()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new TransfersController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            }
        };

        var result = await controller.GetTransferCounts();

        Assert.IsType<UnauthorizedResult>(result.Result);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetTransferCountsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static TransfersController BuildController(Mock<IMediator> mediatorMock, int userId, string role)
    {
        var controller = new TransfersController(mediatorMock.Object);

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        }, "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return controller;
    }
}
