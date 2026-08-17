using Assura.Application.Features.Requests.Commands;
using Assura.Application.Features.Requests.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the BUGS.md Division Head finding: "Inconsistent division scoping between
// list and detail request endpoints" (GetRequestByIdQuery had no division filter at
// all, unlike GetRequestsQuery) and "ReviewRequestByDivisionHeadCommand has no
// division-ownership check and no status guard" (a head could review another
// division's request, or re-review one already resolved).
public class DivisionHeadRequestScopingTests
{
    private static (User requester, User head) MakeUsers(int requesterDivisionId, int headDivisionId) => (
        new User { Id = 1, FirstName = "Req", LastName = "Uester", DivisionId = requesterDivisionId, Role = UserRole.Employee },
        new User { Id = 2, FirstName = "Head", LastName = "Person", DivisionId = headDivisionId, Role = UserRole.DivisionHead }
    );

    // --- GetRequestByIdQuery ---

    [Fact]
    public async Task GetRequestById_DivisionHead_SameDivision_ReturnsRequest()
    {
        using var db = TestContextFactory.CreateContext();
        var (requester, head) = MakeUsers(requesterDivisionId: 5, headDivisionId: 5);
        db.Users.AddRange(requester, head);
        db.Requests.Add(new Request { Id = 1, RequestNumber = "REQ-1", Type = RequestType.Asset, RequesterId = requester.Id, Status = RequestWorkflowStatus.PendingDivisionHeadApproval });
        await db.SaveChangesAsync();

        var handler = new GetRequestByIdQueryHandler(db);
        var result = await handler.Handle(new GetRequestByIdQuery(1, UserId: head.Id, Role: UserRole.DivisionHead), CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetRequestById_DivisionHead_DifferentDivision_ReturnsNull()
    {
        using var db = TestContextFactory.CreateContext();
        var (requester, head) = MakeUsers(requesterDivisionId: 5, headDivisionId: 9);
        db.Users.AddRange(requester, head);
        db.Requests.Add(new Request { Id = 2, RequestNumber = "REQ-2", Type = RequestType.Asset, RequesterId = requester.Id, Status = RequestWorkflowStatus.PendingDivisionHeadApproval });
        await db.SaveChangesAsync();

        var handler = new GetRequestByIdQueryHandler(db);
        var result = await handler.Handle(new GetRequestByIdQuery(2, UserId: head.Id, Role: UserRole.DivisionHead), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRequestById_DivisionHead_DifferentDivision_LegacyAssetRequest_ReturnsNull()
    {
        using var db = TestContextFactory.CreateContext();
        var (requester, head) = MakeUsers(requesterDivisionId: 5, headDivisionId: 9);
        db.Users.AddRange(requester, head);
        db.AssetRequests.Add(new AssetRequest
        {
            Id = 3,
            AssetName = "Laptop",
            Priority = "Normal",
            RequesterId = requester.Id.ToString(),
            RequesterName = "Req Uester",
            RequestType = "NewAsset",
            UserId = requester.Id,
            DivisionId = 5
        });
        await db.SaveChangesAsync();

        var handler = new GetRequestByIdQueryHandler(db);
        // Negative id addresses the legacy AssetRequest table (see handler convention).
        var result = await handler.Handle(new GetRequestByIdQuery(-3, UserId: head.Id, Role: UserRole.DivisionHead), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRequestById_Admin_CanViewAnyDivision()
    {
        using var db = TestContextFactory.CreateContext();
        var (requester, _) = MakeUsers(requesterDivisionId: 5, headDivisionId: 9);
        db.Users.Add(requester);
        db.Requests.Add(new Request { Id = 4, RequestNumber = "REQ-4", Type = RequestType.Asset, RequesterId = requester.Id, Status = RequestWorkflowStatus.PendingDivisionHeadApproval });
        await db.SaveChangesAsync();

        var handler = new GetRequestByIdQueryHandler(db);
        var result = await handler.Handle(new GetRequestByIdQuery(4, UserId: 999, Role: UserRole.Admin), CancellationToken.None);

        Assert.NotNull(result);
    }

    // --- ReviewRequestByDivisionHeadCommand ---

    [Fact]
    public async Task Review_SameDivisionHead_Succeeds()
    {
        using var db = TestContextFactory.CreateContext();
        var (requester, head) = MakeUsers(requesterDivisionId: 5, headDivisionId: 5);
        db.Users.AddRange(requester, head);
        db.Requests.Add(new Request { Id = 5, RequestNumber = "REQ-5", Type = RequestType.Asset, RequesterId = requester.Id, Status = RequestWorkflowStatus.PendingDivisionHeadApproval });
        await db.SaveChangesAsync();

        var handler = new ReviewRequestByDivisionHeadCommandHandler(db);
        var result = await handler.Handle(new ReviewRequestByDivisionHeadCommand { Id = 5, Approve = true, ReviewedByUserId = head.Id }, CancellationToken.None);

        Assert.Equal(ReviewRequestByDivisionHeadResult.Success, result);
        Assert.Equal(RequestWorkflowStatus.PendingStorekeeperReview, (await db.Requests.FindAsync(5))!.Status);
    }

    [Fact]
    public async Task Review_DifferentDivisionHead_ReturnsForbidden()
    {
        using var db = TestContextFactory.CreateContext();
        var (requester, head) = MakeUsers(requesterDivisionId: 5, headDivisionId: 9);
        db.Users.AddRange(requester, head);
        db.Requests.Add(new Request { Id = 6, RequestNumber = "REQ-6", Type = RequestType.Asset, RequesterId = requester.Id, Status = RequestWorkflowStatus.PendingDivisionHeadApproval });
        await db.SaveChangesAsync();

        var handler = new ReviewRequestByDivisionHeadCommandHandler(db);
        var result = await handler.Handle(new ReviewRequestByDivisionHeadCommand { Id = 6, Approve = true, ReviewedByUserId = head.Id }, CancellationToken.None);

        Assert.Equal(ReviewRequestByDivisionHeadResult.Forbidden, result);
        Assert.Equal(RequestWorkflowStatus.PendingDivisionHeadApproval, (await db.Requests.FindAsync(6))!.Status);
    }

    [Fact]
    public async Task Review_Admin_BypassesDivisionCheck()
    {
        using var db = TestContextFactory.CreateContext();
        var (requester, _) = MakeUsers(requesterDivisionId: 5, headDivisionId: 9);
        db.Users.Add(requester);
        db.Requests.Add(new Request { Id = 7, RequestNumber = "REQ-7", Type = RequestType.Asset, RequesterId = requester.Id, Status = RequestWorkflowStatus.PendingDivisionHeadApproval });
        await db.SaveChangesAsync();

        var handler = new ReviewRequestByDivisionHeadCommandHandler(db);
        var result = await handler.Handle(new ReviewRequestByDivisionHeadCommand { Id = 7, Approve = true, ReviewedByUserId = 999, IsAdmin = true }, CancellationToken.None);

        Assert.Equal(ReviewRequestByDivisionHeadResult.Success, result);
    }

    [Fact]
    public async Task Review_AlreadyDecidedRequest_ReturnsInvalidStatus()
    {
        using var db = TestContextFactory.CreateContext();
        var (requester, head) = MakeUsers(requesterDivisionId: 5, headDivisionId: 5);
        db.Users.AddRange(requester, head);
        db.Requests.Add(new Request { Id = 8, RequestNumber = "REQ-8", Type = RequestType.Asset, RequesterId = requester.Id, Status = RequestWorkflowStatus.Rejected });
        await db.SaveChangesAsync();

        var handler = new ReviewRequestByDivisionHeadCommandHandler(db);
        var result = await handler.Handle(new ReviewRequestByDivisionHeadCommand { Id = 8, Approve = true, ReviewedByUserId = head.Id }, CancellationToken.None);

        Assert.Equal(ReviewRequestByDivisionHeadResult.InvalidStatus, result);
    }

    [Fact]
    public async Task Review_UnknownId_ReturnsNotFound()
    {
        using var db = TestContextFactory.CreateContext();

        var handler = new ReviewRequestByDivisionHeadCommandHandler(db);
        var result = await handler.Handle(new ReviewRequestByDivisionHeadCommand { Id = 999, Approve = true, ReviewedByUserId = 1 }, CancellationToken.None);

        Assert.Equal(ReviewRequestByDivisionHeadResult.NotFound, result);
    }

    [Fact]
    public async Task Review_TransferRequest_ScopedByAssetDivision_NotRequesterDivision()
    {
        // A Transfer request's relevant division is the asset's division, not the
        // requester's — matches GetRequestsQueryHandler's special-case for transfers.
        using var db = TestContextFactory.CreateContext();
        var requester = new User { Id = 1, FirstName = "Req", LastName = "Uester", DivisionId = 5, Role = UserRole.Employee };
        var head = new User { Id = 2, FirstName = "Head", LastName = "Person", DivisionId = 9, Role = UserRole.DivisionHead };
        var division = new Division { Id = 9, Name = "Target" };
        var category = new Category { Id = 1, Name = "Cat" };
        var product = new Product { Id = 1, Name = "Prod" };
        var supplier = new Supplier { Id = 1, Name = "Sup" };
        var asset = new Asset
        {
            Id = 1,
            AssetCode = "A-1",
            AssetDate = DateTime.UtcNow,
            Status = AssetStatus.InStore,
            PurchaseValue = 1,
            CategoryId = category.Id,
            DivisionId = 9,
            ProductId = product.Id,
            SupplierId = supplier.Id,
            Category = category,
            Product = product,
            Division = division,
            Supplier = supplier
        };
        db.Users.AddRange(requester, head);
        db.Divisions.Add(division);
        db.Categories.Add(category);
        db.Products.Add(product);
        db.Suppliers.Add(supplier);
        db.Assets.Add(asset);
        db.Requests.Add(new Request { Id = 9, RequestNumber = "REQ-9", Type = RequestType.Transfer, RequesterId = requester.Id, AssetId = asset.Id, Status = RequestWorkflowStatus.PendingDivisionHeadApproval });
        await db.SaveChangesAsync();

        var handler = new ReviewRequestByDivisionHeadCommandHandler(db);
        var result = await handler.Handle(new ReviewRequestByDivisionHeadCommand { Id = 9, Approve = true, ReviewedByUserId = head.Id }, CancellationToken.None);

        Assert.Equal(ReviewRequestByDivisionHeadResult.Success, result);
    }
}
