using Assura.API.Controllers;
using Assura.Application.Features.Assets.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Assura.API.Tests;

public class ReportingControllerVerifyAssetTests
{
    [Fact]
    public async Task VerifyAsset_WhenAssetNotFound_ReturnsNotFound()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<VerifyAssetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = new ReportingController(mediatorMock.Object);

        var result = await controller.VerifyAsset(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task VerifyAsset_WhenAssetExists_ReturnsOkTrue()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<VerifyAssetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new ReportingController(mediatorMock.Object);

        var result = await controller.VerifyAsset(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(true, okResult.Value);
    }
}
