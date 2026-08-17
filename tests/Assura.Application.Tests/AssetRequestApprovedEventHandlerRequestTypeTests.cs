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

    // Covers the bug reported directly by a user: after a Division Head approves a
    // Maintenance request, the Storekeeper cannot see it anywhere and so can't
    // escalate it to Procurement to create a Maintenance Note. Root cause: employees
    // submit maintenance requests through the AssetRequest entity
    // (employee/maintenance-form -> AssetRequestService), but unlike the sibling
    // `Request` entity's ReviewRequestByDivisionHeadCommand (which creates a
    // Maintenance row the moment the head approves), this handler only ever sent a
    // notification — no Maintenance row was created until a Storekeeper separately
    // marked the request "not in stock" via a completely different page, so the
    // Storekeeper's actual Maintenance queue (GetMaintenancesQuery) and the
    // Escalate-to-Procurement action (which requires an existing Maintenance row)
    // never had anything to show or act on.
    [Fact]
    public async Task Handle_MaintenanceRequest_CreatesMaintenanceRecordVisibleToStorekeeper()
    {
        using var db = TestContextFactory.CreateContext();

        var division = new Division { Id = 1, Name = "IT" };
        var asset = new Asset { Id = 10, AssetCode = "AST-10", DivisionId = division.Id };
        var storekeeper = new User { Id = 2, FirstName = "Store", LastName = "Keeper", Role = UserRole.Storekeeper };
        db.Divisions.Add(division);
        db.Assets.Add(asset);
        db.Users.Add(storekeeper);
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 60,
            AssetName = "WF Test Laptop",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "IT Employee",
            RequestType = "Maintenance",
            Description = "Screen is broken",
            UserId = 1,
            AssetId = asset.Id,
            DivisionId = division.Id
        });
        await db.SaveChangesAsync();

        var handler = new AssetRequestApprovedEventHandler(db, Mock.Of<ILogger<AssetRequestApprovedEventHandler>>());
        await handler.Handle(BuildEvent(60, "Maintenance") with { ApprovedByUserId = 3 }, CancellationToken.None);

        var maintenance = Assert.Single(db.Maintenances);
        Assert.Equal(asset.Id, maintenance.AssetId);
        Assert.Equal("Approved", maintenance.Status);
        Assert.Equal(1, maintenance.RequestedByUserId);
        Assert.Equal(3, maintenance.ApprovedByUserId);
        Assert.Equal(60, maintenance.OriginalRequestId); // links back to the AssetRequest that raised it
    }

    // Covers a display gap reported directly by a user: DiscardedNote never recorded
    // which employee raised the discard request, so Superintendent/Admin could see
    // the originating division but not who to follow up with.
    [Fact]
    public async Task Handle_DiscardRequest_RecordsRequesterOnDiscardedNote()
    {
        using var db = TestContextFactory.CreateContext();

        var division = new Division { Id = 1, Name = "IT" };
        db.Divisions.Add(division);
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 53,
            AssetName = "WF Test Laptop",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "IT Employee",
            RequestType = "Discard",
            UserId = 1,
            DivisionId = division.Id
        });
        await db.SaveChangesAsync();

        var handler = new AssetRequestApprovedEventHandler(db, Mock.Of<ILogger<AssetRequestApprovedEventHandler>>());
        await handler.Handle(BuildEvent(53, "Discard") with { RequesterId = "1", RequesterName = "IT Employee" }, CancellationToken.None);

        var note = Assert.Single(db.DiscardedNotes);
        Assert.Equal(1, note.RequestedByUserId);
        Assert.Equal("IT Employee", note.RequestedByName);
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
    public async Task Handle_NewAssetRequest_DoesNotCreateAssetInforming_NotifiesStorekeepers()
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

        Assert.Empty(db.AssetInformings);

        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == storekeeper.Id);
        Assert.NotNull(notification);
        Assert.Contains("awaiting procurement", notification!.Message);
    }

    // Regression guard for a bug the live test-workflow simulation caught that this
    // file's in-memory-database tests could not: Maintenance.OriginalRequestId used
    // to carry a real foreign key to the Requests table (added by migration
    // 20260510111058_EnterpriseMaintenanceWorkflowFix, via a now-removed
    // `Maintenance.OriginalRequest` navigation property), while the handler above
    // unconditionally sets it from an AssetRequest's Id — a different table/ID-space
    // entirely. Every approval of a Maintenance-type AssetRequest threw
    // DbUpdateException against the real MySQL database (silently swallowed by this
    // handler's catch block), so the Maintenance record — and the Storekeeper's only
    // way to see the request — was never created. The in-memory provider used above
    // doesn't enforce foreign keys, so none of the tests in this file could catch it;
    // this test instead asserts directly on the EF model that no such FK exists.
    [Fact]
    public void MaintenanceOriginalRequestId_HasNoForeignKeyConstraint()
    {
        using var db = TestContextFactory.CreateContext();

        var maintenanceType = db.Model.FindEntityType(typeof(Maintenance));
        Assert.NotNull(maintenanceType);

        var foreignKeysOnOriginalRequestId = maintenanceType!.GetForeignKeys()
            .Where(fk => fk.Properties.Any(p => p.Name == nameof(Maintenance.OriginalRequestId)));

        Assert.Empty(foreignKeysOnOriginalRequestId);
    }
}
