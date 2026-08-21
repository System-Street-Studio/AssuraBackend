using Assura.Application.Features.AssetRequests.Commands;
using Assura.Application.Features.Transfers.Commands;
using Assura.Application.Features.Transfers.Handlers;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Xunit;

namespace Assura.Application.Tests;

public class UnderMaintenanceRestrictionTests
{
    [Fact]
    public async Task CreateAssetRequest_Discard_Throws_WhenAssetIsUnderMaintenance()
    {
        using var db = TestContextFactory.CreateContext();
        db.Users.Add(new User { Id = 10, FirstName = "Test", LastName = "User" });
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.UnderMaintenance, AssignedUserId = 10 });
        await db.SaveChangesAsync();

        var handler = new CreateAssetRequestHandler(db);
        var command = new CreateAssetRequestCommand
        {
            EmployeeId = "10",
            SubmittedBy = "Test User",
            AssetName = "Laptop",
            Priority = "Normal",
            RequestType = "Discard",
            AssetId = 1,
            Reason = "Broken beyond repair"
        };

        var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("Assets under maintenance cannot be discarded", ex.Message);
    }

    [Fact]
    public async Task CreateAssetRequest_Transfer_Throws_WhenAssetIsUnderMaintenance()
    {
        using var db = TestContextFactory.CreateContext();
        db.Users.Add(new User { Id = 10, FirstName = "Test", LastName = "User" });
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.UnderMaintenance, AssignedUserId = 10 });
        await db.SaveChangesAsync();

        var handler = new CreateAssetRequestHandler(db);
        var command = new CreateAssetRequestCommand
        {
            EmployeeId = "10",
            SubmittedBy = "Test User",
            AssetName = "Laptop",
            Priority = "Normal",
            RequestType = "Transfer",
            AssetId = 1,
            Reason = "Transferring to other team"
        };

        var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("Assets under maintenance cannot be transferred", ex.Message);
    }

    [Fact]
    public async Task CreateTransfer_Throws_WhenAssetIsUnderMaintenance()
    {
        using var db = TestContextFactory.CreateContext();
        db.Users.Add(new User { Id = 10, Role = UserRole.Employee, DivisionId = 1 });
        db.Users.Add(new User { Id = 20, Role = UserRole.Employee, DivisionId = 2 });
        db.Users.Add(new User { Id = 30, Role = UserRole.DivisionHead, DivisionId = 2 });

        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.UnderMaintenance, AssignedUserId = 10 });
        db.AssetRequests.Add(new AssetRequest { Id = 1, RequesterId = "20", Reason = "Need asset" });
        await db.SaveChangesAsync();

        var handler = new CreateTransferCommandHandler(db);
        var command = new CreateTransferCommand
        {
            AssetId = 1,
            AssetRequestId = 1,
            UserId = 30
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("under maintenance and cannot be transferred", ex.Message);
    }

    [Fact]
    public async Task ConfirmTransferByHead_Throws_WhenAssetIsUnderMaintenance()
    {
        using var db = TestContextFactory.CreateContext();
        db.Transfers.Add(new Transfer
        {
            Id = 1,
            TransferNumber = "TRF-100",
            TransferDate = DateTime.UtcNow,
            AssetRequestId = 1,
            AssetId = 1,
            FromDivisionId = 5,
            ToDivisionId = 9,
            TargetUserId = 77,
            CurrentHolderId = 55,
            Status = TransferStatus.WaitingForFinalConfirmation
        });
        db.Assets.Add(new Asset { Id = 1, AssetCode = "A1", Status = AssetStatus.UnderMaintenance, AssignedUserId = 55 });
        db.Users.Add(new User { Id = 100, Role = UserRole.DivisionHead, DivisionId = 9 });
        await db.SaveChangesAsync();

        var handler = new ConfirmTransferByHeadCommandHandler(db);
        var command = new ConfirmTransferByHeadCommand(1, UserId: 100);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("currently under maintenance and cannot be transferred", ex.Message);
    }
}
