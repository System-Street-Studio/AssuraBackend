using Assura.Application.Features.Requests.Commands;
using Assura.Application.Features.Requests.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests;

public class RequestsWorkflowTests
{
    [Fact]
    public async Task FullHappyPath_CreateApproveReserveConfirm_ShouldFinalizeAssignment()
    {
        using var db = CreateContext();

        var division = new Division { Id = 10, Name = "IT" };
        var category = new Category { Id = 10, Name = "Laptops" };
        var product = new Product { Id = 10, Name = "ThinkPad T14" };
        var supplier = new Supplier { Id = 10, Name = "Tech Source" };

        var employee = new User
        {
            Id = 100,
            Username = "emp_it",
            FirstName = "Emp",
            LastName = "IT",
            Email = "emp@example.com",
            PasswordHash = "x",
            DivisionId = division.Id,
            Role = UserRole.Employee
        };

        var divisionHead = new User
        {
            Id = 101,
            Username = "head_it",
            FirstName = "Head",
            LastName = "IT",
            Email = "head@example.com",
            PasswordHash = "x",
            DivisionId = division.Id,
            Role = UserRole.DivisionHead
        };

        var storekeeper = new User
        {
            Id = 102,
            Username = "storekeeper",
            FirstName = "Store",
            LastName = "Keeper",
            Email = "store@example.com",
            PasswordHash = "x",
            DivisionId = division.Id,
            Role = UserRole.Storekeeper
        };

        var asset = new Asset
        {
            Id = 103,
            AssetCode = "AST-103",
            AssetDate = DateTime.UtcNow,
            Status = AssetStatus.InStore,
            PurchaseValue = 250000,
            CategoryId = category.Id,
            DivisionId = division.Id,
            ProductId = product.Id,
            SupplierId = supplier.Id,
            Category = category,
            Product = product,
            Division = division,
            Supplier = supplier
        };

        db.Divisions.Add(division);
        db.Categories.Add(category);
        db.Products.Add(product);
        db.Suppliers.Add(supplier);
        db.Users.AddRange(employee, divisionHead, storekeeper);
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        var createHandler = new CreateRequestCommandHandler(db);
        var requestId = await createHandler.Handle(new CreateRequestCommand
        {
            Type = RequestType.Asset,
            Priority = PriorityType.Medium,
            Description = "Need laptop for development",
            RequesterId = employee.Id,
            AssetId = asset.Id
        }, CancellationToken.None);

        var created = await db.Requests.FirstAsync(r => r.Id == requestId);
        Assert.Equal(RequestWorkflowStatus.PendingDivisionHeadApproval, created.Status);

        var reviewHandler = new ReviewRequestByDivisionHeadCommandHandler(db);
        await reviewHandler.Handle(new ReviewRequestByDivisionHeadCommand
        {
            Id = requestId,
            Approve = true,
            ReviewedByUserId = divisionHead.Id,
            Remarks = "Approved by division head"
        }, CancellationToken.None);

        var afterHead = await db.Requests.FirstAsync(r => r.Id == requestId);
        Assert.Equal(RequestWorkflowStatus.PendingStorekeeperReview, afterHead.Status);

        var processHandler = new ProcessRequestCommandHandler(db);
        await processHandler.Handle(new ProcessRequestCommand
        {
            Id = requestId,
            IsInStock = true,
            AssetId = asset.Id,
            ProcessedByUserId = storekeeper.Id,
            Remarks = "Reserved"
        }, CancellationToken.None);

        var afterReserve = await db.Requests.FirstAsync(r => r.Id == requestId);
        var reservedAsset = await db.Assets.FirstAsync(a => a.Id == asset.Id);

        Assert.Equal(RequestWorkflowStatus.TemporaryAssigned, afterReserve.Status);
        Assert.Equal(employee.Id, reservedAsset.ReservedForUserId);
        Assert.Equal(requestId, reservedAsset.ReservedByRequestId);

        var confirmHandler = new ConfirmTemporaryAssignmentCommandHandler(db);
        await confirmHandler.Handle(new ConfirmTemporaryAssignmentCommand
        {
            Id = requestId,
            ConfirmedByUserId = storekeeper.Id,
            Remarks = "Physical handover done"
        }, CancellationToken.None);

        var finalized = await db.Requests.FirstAsync(r => r.Id == requestId);
        var assignedAsset = await db.Assets.FirstAsync(a => a.Id == asset.Id);

        Assert.Equal(RequestWorkflowStatus.Approved, finalized.Status);
        Assert.True(finalized.PickupConfirmedAt.HasValue);
        Assert.Equal(employee.Id, assignedAsset.AssignedUserId);
        Assert.Equal(AssetStatus.InUse, assignedAsset.Status);
        Assert.Null(assignedAsset.ReservedForUserId);
        Assert.Null(assignedAsset.ReservedByRequestId);
        Assert.Null(assignedAsset.ReservedUntilUtc);
    }

    [Fact]
    public async Task GetSuggestedAssetsForRequestQuery_ReturnsRankedInStoreCandidates()
    {
        using var db = CreateContext();

        var category = new Category { Id = 1, Name = "Laptops" };
        var productA = new Product { Id = 1, Name = "ThinkPad T14" };
        var productB = new Product { Id = 2, Name = "Latitude" };
        var division = new Division { Id = 1, Name = "IT" };
        var supplier = new Supplier { Id = 1, Name = "TechSource" };

        var requester = new User
        {
            Id = 11,
            Username = "emp_it",
            FirstName = "Emp",
            LastName = "IT",
            Email = "emp@example.com",
            PasswordHash = "x",
            DivisionId = division.Id,
            Role = UserRole.Employee
        };

        var requestedAsset = new Asset
        {
            Id = 101,
            AssetCode = "AST-REQ-101",
            AssetDate = DateTime.UtcNow,
            Status = AssetStatus.InStore,
            PurchaseValue = 300000,
            CategoryId = category.Id,
            DivisionId = division.Id,
            ProductId = productA.Id,
            SupplierId = supplier.Id,
            Category = category,
            Product = productA,
            Division = division,
            Supplier = supplier
        };

        var strongCandidate = new Asset
        {
            Id = 201,
            AssetCode = "AST-201",
            AssetDate = DateTime.UtcNow,
            Status = AssetStatus.InStore,
            PurchaseValue = 290000,
            CategoryId = category.Id,
            DivisionId = division.Id,
            ProductId = productA.Id,
            SupplierId = supplier.Id,
            Category = category,
            Product = productA,
            Division = division,
            Supplier = supplier
        };

        var weakCandidate = new Asset
        {
            Id = 202,
            AssetCode = "AST-202",
            AssetDate = DateTime.UtcNow,
            Status = AssetStatus.InStore,
            PurchaseValue = 250000,
            CategoryId = category.Id,
            DivisionId = division.Id,
            ProductId = productB.Id,
            SupplierId = supplier.Id,
            Category = category,
            Product = productB,
            Division = division,
            Supplier = supplier
        };

        var excludedReserved = new Asset
        {
            Id = 203,
            AssetCode = "AST-203",
            AssetDate = DateTime.UtcNow,
            Status = AssetStatus.InStore,
            PurchaseValue = 280000,
            CategoryId = category.Id,
            DivisionId = division.Id,
            ProductId = productA.Id,
            SupplierId = supplier.Id,
            ReservedForUserId = 999,
            ReservedUntilUtc = DateTime.UtcNow.AddHours(3),
            Category = category,
            Product = productA,
            Division = division,
            Supplier = supplier
        };

        db.Categories.Add(category);
        db.Products.AddRange(productA, productB);
        db.Divisions.Add(division);
        db.Suppliers.Add(supplier);
        db.Users.Add(requester);
        db.Assets.AddRange(requestedAsset, strongCandidate, weakCandidate, excludedReserved);

        db.Requests.Add(new Request
        {
            Id = 301,
            RequestNumber = "REQ-1",
            Type = RequestType.Asset,
            Priority = PriorityType.Medium,
            RequesterId = requester.Id,
            Requester = requester,
            AssetId = requestedAsset.Id,
            Asset = requestedAsset,
            Description = "Need ThinkPad T14 for development"
        });

        await db.SaveChangesAsync();

        var handler = new GetSuggestedAssetsForRequestQueryHandler(db);
        var result = await handler.Handle(new GetSuggestedAssetsForRequestQuery(301), CancellationToken.None);

        Assert.NotEmpty(result);
        Assert.Contains(result, r => r.Id == 201);
        Assert.True(result.First().Score >= result.Last().Score);
        Assert.DoesNotContain(result, r => r.Id == 203);
    }

    [Fact]
    public async Task ProcessRequestCommand_InStock_ReservesAssetAndMovesStatusToTemporaryAssigned()
    {
        using var db = CreateContext();

        var division = new Division { Id = 1, Name = "IT" };
        var category = new Category { Id = 1, Name = "Laptops" };
        var product = new Product { Id = 1, Name = "ThinkPad" };
        var supplier = new Supplier { Id = 1, Name = "TechSource" };

        var requester = new User
        {
            Id = 21,
            Username = "emp_it",
            FirstName = "Emp",
            LastName = "IT",
            Email = "emp@example.com",
            PasswordHash = "x",
            DivisionId = 1,
            Role = UserRole.Employee
        };

        var candidateAsset = new Asset
        {
            Id = 401,
            AssetCode = "AST-401",
            AssetDate = DateTime.UtcNow,
            Status = AssetStatus.InStore,
            PurchaseValue = 200000,
            CategoryId = category.Id,
            DivisionId = division.Id,
            ProductId = product.Id,
            SupplierId = supplier.Id,
            Category = category,
            Product = product,
            Division = division,
            Supplier = supplier
        };

        var request = new Request
        {
            Id = 501,
            RequestNumber = "REQ-501",
            Type = RequestType.Asset,
            Priority = PriorityType.Medium,
            RequesterId = requester.Id,
            Requester = requester,
            Status = RequestWorkflowStatus.PendingStorekeeperReview
        };

        db.Divisions.Add(division);
        db.Categories.Add(category);
        db.Products.Add(product);
        db.Suppliers.Add(supplier);
        db.Users.Add(requester);
        db.Assets.Add(candidateAsset);
        db.Requests.Add(request);
        await db.SaveChangesAsync();

        var handler = new ProcessRequestCommandHandler(db);
        await handler.Handle(new ProcessRequestCommand
        {
            Id = request.Id,
            IsInStock = true,
            AssetId = candidateAsset.Id,
            Remarks = "Reserved by storekeeper",
            ProcessedByUserId = 999
        }, CancellationToken.None);

        var savedRequest = await db.Requests.FirstAsync(r => r.Id == request.Id);
        var savedAsset = await db.Assets.FirstAsync(a => a.Id == candidateAsset.Id);

        Assert.Equal(RequestWorkflowStatus.TemporaryAssigned, savedRequest.Status);
        Assert.Equal(candidateAsset.Id, savedRequest.AssetId);
        Assert.Equal(requester.Id, savedAsset.ReservedForUserId);
        Assert.Equal(request.Id, savedAsset.ReservedByRequestId);
        Assert.True(savedAsset.ReservedUntilUtc.HasValue);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
