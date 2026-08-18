using Assura.Application.Features.DiscardedNotes.Queries.GetAll;
using Assura.Application.Features.QueueItems.Queries.GetAll;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Assura.Application.Tests;

// Covers the BUGS.md Superintendent finding: "QueueItemDto never returns ReviewNote,
// even though the entity has it and the Superintendent's own UI writes one." Also adds
// the missing coverage noted in "No test coverage for Superintendent-specific backend
// logic" for GetAllQueueItemsQueryHandler and GetAllDiscardedNotesQueryHandler, which
// previously had zero tests.
public class GetAllQueueItemsAndDiscardedNotesTests
{
    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { }, typeof(Assura.Application.DependencyInjection).Assembly);
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [Fact]
    public async Task GetAllQueueItemsQueryHandler_ShouldReturnReviewNote()
    {
        using var db = TestContextFactory.CreateContext();
        var mapper = CreateMapper();

        db.QueueItems.Add(new QueueItem
        {
            Name = "Old Printer",
            Division = "IT",
            AssetType = "Hardware",
            Status = QueueItemStatus.Approved,
            SpecialNote = "Requested for disposal",
            ReviewNote = "Approved after physical inspection"
        });
        await db.SaveChangesAsync();

        var handler = new GetAllQueueItemsQueryHandler(db, mapper);
        var result = await handler.Handle(new GetAllQueueItemsQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("Approved after physical inspection", dto.ReviewNote);
    }

    [Fact]
    public async Task GetAllQueueItemsQueryHandler_ShouldReturnAllSeededItems()
    {
        using var db = TestContextFactory.CreateContext();
        var mapper = CreateMapper();

        db.QueueItems.Add(new QueueItem { Name = "Chair", Division = "HR", AssetType = "Furniture", Status = QueueItemStatus.Pending });
        db.QueueItems.Add(new QueueItem { Name = "Laptop", Division = "IT", AssetType = "Hardware", Status = QueueItemStatus.Unread });
        await db.SaveChangesAsync();

        var handler = new GetAllQueueItemsQueryHandler(db, mapper);
        var result = await handler.Handle(new GetAllQueueItemsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllDiscardedNotesQueryHandler_ShouldReturnAllSeededNotes()
    {
        using var db = TestContextFactory.CreateContext();
        var mapper = CreateMapper();

        db.DiscardedNotes.Add(new DiscardedNote { Name = "Broken Chair", Division = "Facilities", AssetType = "Furniture", Status = DiscardNoteStatus.Completed });
        await db.SaveChangesAsync();

        var handler = new GetAllDiscardedNotesQueryHandler(db, mapper);
        var result = await handler.Handle(new GetAllDiscardedNotesQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("Broken Chair", dto.Name);
        Assert.Equal("Completed", dto.Status);
    }

    // Covers the new "show the assignee of the asset to be discarded" requirement
    // requested alongside the WORKFLOW_BASELINE_discarding.md fixes: the Superintendent
    // reviewing a discard note had no way to see who the asset was assigned to, only who
    // requested the discard.
    [Fact]
    public async Task GetAllDiscardedNotesQueryHandler_ShouldReturnAssigneeName_FromLinkedAsset()
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

        db.DiscardedNotes.Add(new DiscardedNote { Name = "Broken Chair", Division = "Facilities", AssetType = "Furniture", Status = DiscardNoteStatus.Completed, AssetId = asset.Id });
        await db.SaveChangesAsync();

        var handler = new GetAllDiscardedNotesQueryHandler(db, mapper);
        var result = await handler.Handle(new GetAllDiscardedNotesQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("IT Employee", dto.AssigneeName);
    }
}
