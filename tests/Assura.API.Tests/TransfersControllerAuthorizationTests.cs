using System.Security.Claims;
using Assura.API.Controllers;
using Assura.Application.Features.Transfers.Commands;
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

    // Covers the BUGS.md finding: CreateTransfer trusted a client-supplied UserId in
    // the request body (spoofable to any user id) instead of deriving it from the
    // caller's own JWT, the way every other action on this controller already does.
    [Fact]
    public async Task CreateTransfer_UsesCallerIdFromJwt_NotRequestBody()
    {
        var mediatorMock = new Mock<IMediator>();
        CreateTransferCommand? captured = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateTransferCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<int>, CancellationToken>((c, _) => captured = (CreateTransferCommand)c)
            .ReturnsAsync(123);
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTransferByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransferDto { Id = 123 });

        var controller = BuildController(mediatorMock, userId: 42, role: "DivisionHead");

        var dto = new CreateTransferDto { AssetId = 1, AssetRequestId = 1, UserId = 999 }; // spoofed id in body
        var result = await controller.CreateTransfer(dto);

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.NotNull(captured);
        Assert.Equal(42, captured!.UserId); // caller's own JWT id, not the spoofed 999
    }

    // Covers the incident where CreateTransferCommandHandler's division check threw
    // UnauthorizedAccessException, but CreateTransfer's action had no catch clause for
    // it (unlike approve-head/confirm-head/etc.) — so a real, legitimate authorization
    // failure fell into the generic catch(Exception) and surfaced to the Division Head
    // as a 500 instead of a 403.
    [Fact]
    public async Task CreateTransfer_UnauthorizedFromHandler_ReturnsForbidden()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateTransferCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("You may only initiate transfers for requests approved within your own division."));

        var controller = BuildController(mediatorMock, userId: 42, role: "DivisionHead");

        var dto = new CreateTransferDto { AssetId = 1, AssetRequestId = 1 };
        var result = await controller.CreateTransfer(dto);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, statusResult.StatusCode);
    }

    // Covers the newly-added GET /api/transfers/asset/{assetId} endpoint — lets an
    // asset's original owner (and anyone else) see its full transfer history, closing
    // the gap where a transferred-away asset just silently disappeared with no trace.
    [Fact]
    public async Task GetTransferHistoryForAsset_SendsCorrectAssetIdFilter()
    {
        var mediatorMock = new Mock<IMediator>();
        GetAllTransfersQuery? captured = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAllTransfersQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<List<TransferDto>>, CancellationToken>((q, _) => captured = (GetAllTransfersQuery)q)
            .ReturnsAsync(new List<TransferDto> { new() { Id = 1, AssetId = 7 } });

        var controller = BuildController(mediatorMock, userId: 42, role: "Employee");

        var result = await controller.GetTransferHistoryForAsset(7);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var history = Assert.IsType<List<TransferDto>>(okResult.Value);
        Assert.Single(history);
        Assert.NotNull(captured);
        Assert.Equal(7, captured!.AssetId);
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
