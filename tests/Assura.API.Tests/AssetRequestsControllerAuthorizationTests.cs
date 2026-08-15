using System.Security.Claims;
using Assura.API.Controllers;
using Assura.Application.Features.AssetRequests.Commands;
using Assura.Application.Features.AssetRequests.DTOs;
using Assura.Application.Features.AssetRequests.Queries;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Assura.API.Tests;

// Covers two BUGS.md Employee findings on AssetRequestsController:
// 1. "IDOR: Employee can view any other employee's full request history via free
//    employeeId URL param" (GetByEmployee).
// 2. "Client-trusted identity on asset request creation — Employee can impersonate
//    another employee" (Create trusted EmployeeId/SubmittedBy straight from the body).
public class AssetRequestsControllerAuthorizationTests
{
    [Fact]
    public async Task GetByEmployee_AsEmployee_RequestingSomeoneElsesId_ReturnsForbidden()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = BuildController(mediatorMock, userId: 1, role: "Employee");

        var result = await controller.GetByEmployee("999");

        Assert.IsType<ForbidResult>(result);
        mediatorMock.Verify(
            m => m.Send(It.IsAny<GetFilteredAssetRequestsQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByEmployee_AsEmployee_RequestingOwnId_Succeeds()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFilteredAssetRequestsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AssetRequestDto>());

        var controller = BuildController(mediatorMock, userId: 1, role: "Employee");

        var result = await controller.GetByEmployee("1");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetByEmployee_AsAdmin_RequestingAnotherEmployeesId_Succeeds()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFilteredAssetRequestsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AssetRequestDto>());

        var controller = BuildController(mediatorMock, userId: 1, role: "Admin");

        var result = await controller.GetByEmployee("999");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Create_IgnoresClientSuppliedEmployeeId_UsesAuthenticatedCallerInstead()
    {
        var mediatorMock = new Mock<IMediator>();
        CreateAssetRequestCommand? captured = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateAssetRequestCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<int>, CancellationToken>((c, _) => captured = (CreateAssetRequestCommand)c)
            .ReturnsAsync(1);

        var controller = BuildController(mediatorMock, userId: 1, role: "Employee");

        var input = new AssetRequestsController.CreateAssetRequestApiInput
        {
            EmployeeId = "999", // attempted impersonation of another employee
            SubmittedBy = "Someone Else",
            AssetName = "Laptop",
            Priority = "Normal",
            RequestType = "NewAsset"
        };

        await controller.Create(input);

        Assert.NotNull(captured);
        Assert.Equal("1", captured!.EmployeeId);
        Assert.NotEqual("999", captured.EmployeeId);
    }

    // Covers the BUGS.md Division Head finding: "Missing role restriction on Asset
    // Request approve/reject". The [Authorize(Roles=...)] attribute itself can't be
    // exercised without a full ASP.NET pipeline, so these confirm the controller now
    // forwards the caller's identity to the command (so the handler can enforce
    // division-scoping) and translates each result onto the correct HTTP status.
    [Fact]
    public async Task Approve_ForwardsCallerIdentity_AndMapsForbiddenToForbidResult()
    {
        var mediatorMock = new Mock<IMediator>();
        ApproveAssetRequestCommand? captured = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<ApproveAssetRequestCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<ApproveAssetRequestResult>, CancellationToken>((c, _) => captured = (ApproveAssetRequestCommand)c)
            .ReturnsAsync(ApproveAssetRequestResult.Forbidden);

        var controller = BuildController(mediatorMock, userId: 42, role: "DivisionHead");

        var result = await controller.Approve(7);

        Assert.IsType<ForbidResult>(result);
        Assert.NotNull(captured);
        Assert.Equal(7, captured!.Id);
        Assert.Equal(42, captured.UserId);
        Assert.False(captured.IsAdmin);
    }

    [Fact]
    public async Task Approve_AsAdmin_SetsIsAdminTrue_AndMapsSuccessToOk()
    {
        var mediatorMock = new Mock<IMediator>();
        ApproveAssetRequestCommand? captured = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<ApproveAssetRequestCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<ApproveAssetRequestResult>, CancellationToken>((c, _) => captured = (ApproveAssetRequestCommand)c)
            .ReturnsAsync(ApproveAssetRequestResult.Success);

        var controller = BuildController(mediatorMock, userId: 1, role: "Admin");

        var result = await controller.Approve(7);

        Assert.IsType<OkObjectResult>(result);
        Assert.True(captured!.IsAdmin);
    }

    [Fact]
    public async Task Approve_AlreadyDecidedRequest_ReturnsConflict()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<ApproveAssetRequestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApproveAssetRequestResult.InvalidStatus);

        var controller = BuildController(mediatorMock, userId: 42, role: "DivisionHead");

        var result = await controller.Approve(7);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Reject_ForwardsCallerIdentity_AndMapsForbiddenToForbidResult()
    {
        var mediatorMock = new Mock<IMediator>();
        RejectAssetRequestCommand? captured = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<RejectAssetRequestCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<RejectAssetRequestResult>, CancellationToken>((c, _) => captured = (RejectAssetRequestCommand)c)
            .ReturnsAsync(RejectAssetRequestResult.Forbidden);

        var controller = BuildController(mediatorMock, userId: 42, role: "DivisionHead");

        var result = await controller.Reject(7);

        Assert.IsType<ForbidResult>(result);
        Assert.NotNull(captured);
        Assert.Equal(7, captured!.Id);
        Assert.Equal(42, captured.UserId);
        Assert.False(captured.IsAdmin);
    }

    // Covers the BUGS.md Division Head finding: "IDOR: GET /api/asset-requests/
    // approved-transfers accepts a client-supplied headId" — omitting it used to leak
    // approved transfers across every division instead of scoping to the caller.
    [Fact]
    public async Task GetApprovedTransfers_AsDivisionHead_UsesCallerIdFromJwt_IgnoringAnyClientInput()
    {
        var mediatorMock = new Mock<IMediator>();
        GetApprovedTransfersQuery? captured = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetApprovedTransfersQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<List<ApprovedTransferRequestDto>>, CancellationToken>((q, _) => captured = (GetApprovedTransfersQuery)q)
            .ReturnsAsync(new List<ApprovedTransferRequestDto>());

        var controller = BuildController(mediatorMock, userId: 42, role: "DivisionHead");

        var result = await controller.GetApprovedTransfers();

        Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(captured);
        Assert.Equal(42, captured!.headId);
    }

    [Fact]
    public async Task GetApprovedTransfers_AsAdmin_SeesAllDivisions()
    {
        var mediatorMock = new Mock<IMediator>();
        GetApprovedTransfersQuery? captured = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetApprovedTransfersQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<List<ApprovedTransferRequestDto>>, CancellationToken>((q, _) => captured = (GetApprovedTransfersQuery)q)
            .ReturnsAsync(new List<ApprovedTransferRequestDto>());

        var controller = BuildController(mediatorMock, userId: 1, role: "Admin");

        await controller.GetApprovedTransfers();

        Assert.Null(captured!.headId);
    }

    private static AssetRequestsController BuildController(Mock<IMediator> mediatorMock, int userId, string role)
    {
        var envMock = new Mock<IWebHostEnvironment>();
        var controller = new AssetRequestsController(mediatorMock.Object, envMock.Object);

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
