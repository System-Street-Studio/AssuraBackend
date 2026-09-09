using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assura.Application.Features.Requests.Commands;
using Assura.Application.PurchasingOrders.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Assura.Application.Tests;

public class ProcessRequestEscalationTests
{
    [Fact]
    public async Task ProcessRequest_OnApprovedAssetRequest_EscalatesToProcurementAndAppearsInQueue()
    {
        using var db = TestContextFactory.CreateContext();

        var storekeeper = new User
        {
            Id = 501,
            FirstName = "Store",
            LastName = "Keeper",
            Role = UserRole.Storekeeper
        };
        var procurementUser = new User
        {
            Id = 502,
            FirstName = "Proc",
            LastName = "Officer",
            Role = UserRole.Procurement
        };
        var employee = new User
        {
            Id = 503,
            FirstName = "Emp",
            LastName = "Loyee",
            Role = UserRole.Employee
        };

        db.Users.AddRange(storekeeper, procurementUser, employee);

        // AssetRequest approved by Division Head has Status == Approved
        var assetReq = new AssetRequest
        {
            Id = 880,
            AssetName = "Office Desk",
            AssetCategory = "Furniture",
            Priority = "Normal",
            RequesterId = employee.Id.ToString(),
            RequesterName = $"{employee.FirstName} {employee.LastName}",
            RequestType = "New Asset",
            Status = RequestStatus.Approved,
            UserId = employee.Id
        };
        db.AssetRequests.Add(assetReq);
        await db.SaveChangesAsync();

        var handler = new ProcessRequestCommandHandler(db);

        // Storekeeper processes as out of stock (escalate to procurement)
        await handler.Handle(new ProcessRequestCommand
        {
            Id = -assetReq.Id,
            IsInStock = false,
            Remarks = "Item not in warehouse, purchase required",
            ProcessedByUserId = storekeeper.Id,
            CallerRole = Roles.Storekeeper
        }, CancellationToken.None);

        // 1. Verify AssetRequest was updated to PendingProcurement
        var updated = await db.AssetRequests.FindAsync(assetReq.Id);
        Assert.NotNull(updated);
        Assert.Equal(RequestStatus.PendingProcurement, updated!.Status);
        Assert.Equal("Store Keeper", updated.ProcessedByName);
        Assert.Equal("Item not in warehouse, purchase required", updated.ProcessorRemarks);

        // 2. Verify Procurement notification was created
        var notif = await db.Notifications
            .FirstOrDefaultAsync(n => n.UserId == procurementUser.Id && n.Title == "Asset Escalated to Procurement");
        Assert.NotNull(notif);
        Assert.Contains("Office Desk", notif!.Message);

        // 3. Verify GetPendingAssetRequestsQuery (Procurement PO queue) includes this request with negated ID
        var pendingHandler = new GetPendingAssetRequestsQueryHandler(db);
        var queue = await pendingHandler.Handle(new GetPendingAssetRequestsQuery(), CancellationToken.None);

        var queueItem = Assert.Single(queue);
        Assert.Equal(-assetReq.Id, queueItem.Id);
        Assert.Equal("Office Desk", queueItem.AssetName);
        Assert.Equal("New Asset", queueItem.Type);
    }

    [Fact]
    public async Task ProcessRequest_OnApprovedRequestTableEntity_EscalatesToProcurementAndAppearsInQueue()
    {
        using var db = TestContextFactory.CreateContext();

        var storekeeper = new User
        {
            Id = 601,
            FirstName = "Store",
            LastName = "Keeper",
            Role = UserRole.Storekeeper
        };
        var requester = new User
        {
            Id = 602,
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Employee
        };

        db.Users.AddRange(storekeeper, requester);

        // Request approved by Division Head or set to Approved
        var req = new Request
        {
            Id = 990,
            RequestNumber = "REQ-990",
            Type = RequestType.Asset,
            Priority = PriorityType.High,
            Description = "Ergonomic Chair",
            Status = RequestWorkflowStatus.Approved,
            RequesterId = requester.Id,
            Requester = requester,
            CreatedAt = DateTime.UtcNow
        };
        db.Requests.Add(req);
        await db.SaveChangesAsync();

        var handler = new ProcessRequestCommandHandler(db);

        await handler.Handle(new ProcessRequestCommand
        {
            Id = req.Id,
            IsInStock = false,
            Remarks = "Out of stock",
            ProcessedByUserId = storekeeper.Id,
            CallerRole = Roles.Storekeeper
        }, CancellationToken.None);

        var updated = await db.Requests.FindAsync(req.Id);
        Assert.NotNull(updated);
        Assert.Equal(RequestWorkflowStatus.PendingProcurement, updated!.Status);

        var pendingHandler = new GetPendingAssetRequestsQueryHandler(db);
        var queue = await pendingHandler.Handle(new GetPendingAssetRequestsQuery(), CancellationToken.None);

        var queueItem = Assert.Single(queue);
        Assert.Equal(req.Id, queueItem.Id);
        Assert.Equal("Asset", queueItem.Type);
    }

    [Fact]
    public async Task ProcessRequest_AlreadyPendingProcurement_IsSafelyIgnored()
    {
        using var db = TestContextFactory.CreateContext();

        var storekeeper = new User
        {
            Id = 701,
            FirstName = "Store",
            LastName = "Keeper",
            Role = UserRole.Storekeeper
        };
        db.Users.Add(storekeeper);

        var assetReq = new AssetRequest
        {
            Id = 881,
            AssetName = "Monitor",
            Status = RequestStatus.PendingProcurement,
            RequesterId = "1",
            RequestType = "New Asset",
            ProcessorRemarks = "Original remarks"
        };
        db.AssetRequests.Add(assetReq);
        await db.SaveChangesAsync();

        var handler = new ProcessRequestCommandHandler(db);

        // Attempt duplicate/re-process
        await handler.Handle(new ProcessRequestCommand
        {
            Id = -assetReq.Id,
            IsInStock = false,
            Remarks = "Duplicate call",
            ProcessedByUserId = storekeeper.Id,
            CallerRole = Roles.Storekeeper
        }, CancellationToken.None);

        var current = await db.AssetRequests.FindAsync(assetReq.Id);
        Assert.NotNull(current);
        // Remains original, not overwritten
        Assert.Equal("Original remarks", current!.ProcessorRemarks);
    }
}
