using Assura.Application.Features.AccPendingItems.Queries.GetAll;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Assura.Application.Tests;

// Covers the new "show the assignee of the asset to be discarded" requirement
// requested alongside the WORKFLOW_BASELINE_discarding.md fixes: the Accountant
// confirming a discard had no way to see who the asset was assigned to, only who
// requested the discard (and even that was null before the requester-identity fix).
public class GetAllAccPendingItemsTests
{
    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { }, typeof(Assura.Application.DependencyInjection).Assembly);
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [Fact]
    public async Task GetAllAccPendingItemsQueryHandler_ShouldReturnAssigneeName_FromLinkedAsset()
    {
        using var db = TestContextFactory.CreateContext();
        var mapper = CreateMapper();

        var assignee = new User { FirstName = "IT", LastName = "Employee", Role = UserRole.Employee };
        db.Users.Add(assignee);
        var asset = new Asset { AssetCode = "AST-0100", Status = AssetStatus.InUse };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        asset.AssignedUserId = assignee.Id;
        await db.SaveChangesAsync();

        db.AccPendingItems.Add(new AccPendingItem
        {
            Name = "Broken Chair",
            Division = "Facilities",
            Status = "Pending",
            Category = AccPendingCategory.Pending,
            AssetType = "Furniture",
            CurrentUser = "Superintendent One",
            AssetId = asset.Id
        });
        await db.SaveChangesAsync();

        var handler = new GetAllAccPendingItemsQueryHandler(db, mapper);
        var result = await handler.Handle(new GetAllAccPendingItemsQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("IT Employee", dto.AssigneeName);
    }

    [Fact]
    public async Task GetAllAccPendingItemsQueryHandler_WithNoLinkedAsset_ReturnsNullAssignee()
    {
        using var db = TestContextFactory.CreateContext();
        var mapper = CreateMapper();

        db.AccPendingItems.Add(new AccPendingItem
        {
            Name = "Legacy Item",
            Division = "Facilities",
            Status = "Pending",
            Category = AccPendingCategory.Pending,
            AssetType = "Furniture",
            CurrentUser = "Superintendent One"
        });
        await db.SaveChangesAsync();

        var handler = new GetAllAccPendingItemsQueryHandler(db, mapper);
        var result = await handler.Handle(new GetAllAccPendingItemsQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Null(dto.AssigneeName);
    }
}
