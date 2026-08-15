using System.Security.Claims;
using Assura.API.Controllers;
using Assura.Application.DTOs;
using Assura.Application.Features.Assets.Queries;
using Assura.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Assura.API.Tests;

public class AssetsControllerRoleFilterTests
{
    [Fact]
    public async Task GetAssets_ForAuditor_ReturnsFullUnfilteredAssetList()
    {
        var mediatorMock = new Mock<IMediator>();
        GetAssetsQuery? capturedQuery = null;
        mediatorMock
            .Setup(m => m.Send(It.IsAny<GetAssetsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<List<AssetDto>>, CancellationToken>((q, _) => capturedQuery = (GetAssetsQuery)q)
            .ReturnsAsync(new List<AssetDto>());

        var controller = new AssetsController(mediatorMock.Object)
        {
            ControllerContext = BuildControllerContext(userId: 42, role: Roles.Auditor)
        };

        await controller.GetAssets();

        Assert.NotNull(capturedQuery);
        Assert.Null(capturedQuery!.AssignedUserId);
        Assert.Null(capturedQuery.RequesterUserId);
    }

    private static ControllerContext BuildControllerContext(int userId, string role)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        }, "TestAuth");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }
}
