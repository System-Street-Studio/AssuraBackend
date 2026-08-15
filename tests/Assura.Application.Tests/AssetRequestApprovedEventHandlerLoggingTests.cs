using Assura.Application.Features.AssetRequests.Events;
using Assura.Application.Tests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace Assura.Application.Tests;

// Covers the BUGS.md Superintendent finding: "AssetRequestApprovedEventHandler
// (originates every Superintendent queue item) swallows failures silently." A broad
// try/catch around the whole handler only did Console.WriteLine on failure, so a
// failure to create the queue item/discard note for a Superintendent to review would
// fail with no trace. This test forces a failure (a disposed DbContext) and asserts
// the handler now reports it through ILogger instead of Console.WriteLine.
public class AssetRequestApprovedEventHandlerLoggingTests
{
    [Fact]
    public async Task Handle_OnFailure_ShouldLogErrorInsteadOfSwallowingSilently()
    {
        var db = TestContextFactory.CreateContext();
        await db.DisposeAsync();

        var loggerMock = new Mock<ILogger<AssetRequestApprovedEventHandler>>();
        var handler = new AssetRequestApprovedEventHandler(db, loggerMock.Object);

        var notification = new AssetRequestApprovedEvent(
            Id: 1,
            AssetName: "Old Printer",
            AssetCategory: "Hardware",
            Quantity: 1,
            RequestType: "Discard",
            Priority: "Normal",
            Status: "Approved",
            RequesterName: "Bob",
            RequesterId: "2",
            Attachments: "",
            SubmittedDate: DateTime.UtcNow,
            Description: "Broken",
            Reason: "End of life");

        await handler.Handle(notification, CancellationToken.None);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
