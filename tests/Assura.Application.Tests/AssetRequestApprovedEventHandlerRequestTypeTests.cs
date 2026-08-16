using Assura.Application.Features.AssetRequests.Events;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Assura.Application.Tests;

// Covers a bug found by the test-workflow simulation: approving a Maintenance (or
// any other non-"New Asset") request was falling into the same branch as a genuine
// new-asset purchase, creating a bogus AssetInforming "new arrival" record (with a
// nonsensical model/price of 0) and telling Storekeepers it was "awaiting
// procurement" — even though the asset already exists and just needs repair.
// Confirmed live: approving a Maintenance request for an existing asset produced a
// fake arrival entry in Procurement's Informed Arrivals queue.
public class AssetRequestApprovedEventHandlerRequestTypeTests
{
    private static AssetRequestApprovedEvent BuildEvent(int id, string requestType) => new(
        Id: id,
        AssetName: "WF Test Laptop",
        AssetCategory: "IT",
        Quantity: 1,
        RequestType: requestType,
        Priority: "Normal",
        Status: "Approved",
        RequesterName: "IT Employee",
        RequesterId: "1",
        Attachments: "N/A",
        SubmittedDate: DateTime.UtcNow,
        Description: "test",
        Reason: "test");

    [Fact]
    public async Task Handle_MaintenanceRequest_DoesNotCreateAssetInformingButNotifiesStorekeepers()
    {
        using var db = TestContextFactory.CreateContext();

        var division = new Division { Id = 1, Name = "IT" };
        var storekeeper = new User { Id = 2, FirstName = "Store", LastName = "Keeper", Role = UserRole.Storekeeper };
        db.Divisions.Add(division);
        db.Users.Add(storekeeper);
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 50,
            AssetName = "WF Test Laptop",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "IT Employee",
            RequestType = "Maintenance",
            UserId = 1,
            DivisionId = division.Id
        });
        await db.SaveChangesAsync();

        var handler = new AssetRequestApprovedEventHandler(db, Mock.Of<ILogger<AssetRequestApprovedEventHandler>>());
        await handler.Handle(BuildEvent(50, "Maintenance"), CancellationToken.None);

        Assert.Empty(db.AssetInformings);
        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == storekeeper.Id);
        Assert.NotNull(notification);
        Assert.Contains("Maintenance", notification!.Message);
        Assert.DoesNotContain("awaiting procurement", notification.Message);
    }

    [Fact]
    public async Task Handle_TransferRequest_DoesNotCreateAssetInforming()
    {
        using var db = TestContextFactory.CreateContext();

        var division = new Division { Id = 1, Name = "IT" };
        db.Divisions.Add(division);
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 51,
            AssetName = "WF Test Laptop",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "IT Employee",
            RequestType = "Transfer",
            UserId = 1,
            DivisionId = division.Id
        });
        await db.SaveChangesAsync();

        var handler = new AssetRequestApprovedEventHandler(db, Mock.Of<ILogger<AssetRequestApprovedEventHandler>>());
        await handler.Handle(BuildEvent(51, "Transfer"), CancellationToken.None);

        Assert.Empty(db.AssetInformings);
    }

    [Fact]
    public async Task Handle_NewAssetRequest_StillCreatesAssetInformingAndNotifiesStorekeepers()
    {
        using var db = TestContextFactory.CreateContext();

        var division = new Division { Id = 1, Name = "IT" };
        var storekeeper = new User { Id = 2, FirstName = "Store", LastName = "Keeper", Role = UserRole.Storekeeper };
        db.Divisions.Add(division);
        db.Users.Add(storekeeper);
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 52,
            AssetName = "WF Test Laptop",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "IT Employee",
            RequestType = "New Asset",
            UserId = 1,
            DivisionId = division.Id
        });
        await db.SaveChangesAsync();

        var handler = new AssetRequestApprovedEventHandler(db, Mock.Of<ILogger<AssetRequestApprovedEventHandler>>());
        await handler.Handle(BuildEvent(52, "New Asset"), CancellationToken.None);

        var informing = Assert.Single(db.AssetInformings);
        Assert.Equal("WF Test Laptop", informing.ItemName);

        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == storekeeper.Id);
        Assert.NotNull(notification);
        Assert.Contains("awaiting procurement", notification!.Message);
    }
}
