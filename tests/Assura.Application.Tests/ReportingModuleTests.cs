using Assura.Application.Features.Reporting.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests;

public class ReportingModuleTests
{
    [Fact]
    public async Task ReportingDashboardQuery_ReturnsAggregatedMetricsAndBars()
    {
        using var db = CreateContext();

        var divisionA = new Division { Id = 1, Name = "IT" };
        var divisionB = new Division { Id = 2, Name = "Finance" };
        var categoryA = new Category { Id = 1, Name = "Laptop" };
        var categoryB = new Category { Id = 2, Name = "Monitor" };
        var productA = new Product { Id = 1, Name = "ThinkPad T14" };
        var productB = new Product { Id = 2, Name = "Dell 27 Monitor" };
        var supplier = new Supplier { Id = 1, Name = "Metro" };

        db.Divisions.AddRange(divisionA, divisionB);
        db.Categories.AddRange(categoryA, categoryB);
        db.Products.AddRange(productA, productB);
        db.Suppliers.Add(supplier);

        db.Assets.AddRange(
            new Asset
            {
                Id = 1,
                AssetCode = "AST-001",
                AssetDate = DateTime.UtcNow.AddYears(-1),
                Status = AssetStatus.InUse,
                PurchaseValue = 150000,
                CategoryId = categoryA.Id,
                DivisionId = divisionA.Id,
                ProductId = productA.Id,
                SupplierId = supplier.Id,
                Category = categoryA,
                Division = divisionA,
                Product = productA,
                Supplier = supplier,
                SerialNumber = "SN-1"
            },
            new Asset
            {
                Id = 2,
                AssetCode = "AST-002",
                AssetDate = DateTime.UtcNow.AddYears(-2),
                Status = AssetStatus.Lost,
                PurchaseValue = 100000,
                CategoryId = categoryB.Id,
                DivisionId = divisionB.Id,
                ProductId = productB.Id,
                SupplierId = supplier.Id,
                Category = categoryB,
                Division = divisionB,
                Product = productB,
                Supplier = supplier
            });

        db.AuditLogs.Add(new AuditLog
        {
            Id = 1,
            EntityName = "Asset",
            EntityId = "AST-002",
            Action = "Delete",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var handler = new GetReportingDashboardQueryHandler(db);
        var result = await handler.Handle(new GetReportingDashboardQuery(), CancellationToken.None);

        Assert.Equal("2", result.Metrics.First(m => m.Label == "Total Assets").Value.Replace(",", ""));
        Assert.Equal(2, result.CategoryLegend.Count);
        Assert.Contains(result.StatusBars, b => b.Label == "Active");
        Assert.Equal(1, result.Anomalies.GhostAssetsDetected);
        Assert.Equal(1, result.Anomalies.MissingPhysicalVerification);
    }

    [Fact]
    public async Task ReportingAuditLogsQuery_MapsActorAndClassifiesStatuses()
    {
        using var db = CreateContext();

        var user = new User
        {
            Id = 1,
            Username = "1",
            FirstName = "Aster",
            LastName = "Mendis",
            Email = "aster@example.com",
            PasswordHash = "hash",
            Role = UserRole.Auditor
        };

        db.Users.Add(user);
        db.AuditLogs.AddRange(
            new AuditLog
            {
                Id = 1,
                EntityName = "Export",
                EntityId = "EXP-1",
                Action = "Export",
                CreatedBy = user.Id.ToString(),
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow
            },
            new AuditLog
            {
                Id = 2,
                EntityName = "Asset",
                EntityId = "AST-2",
                Action = "Delete",
                CreatedBy = user.Username,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            });

        await db.SaveChangesAsync();

        var handler = new GetReportingAuditLogsQueryHandler(db);
        var result = await handler.Handle(new GetReportingAuditLogsQuery(), CancellationToken.None);

        Assert.True(result.Logs.Count >= 2);
        Assert.Contains(result.Logs, l => l.Actor == "Aster Mendis");
        Assert.Contains(result.Logs, l => l.Status == "Completed");
        Assert.Contains(result.Logs, l => l.Status == "Flagged");
    }

    [Fact]
    public async Task ReportingAuditLogsQuery_ClassifiesVerifyActionAsCompleted()
    {
        using var db = CreateContext();

        db.AuditLogs.Add(new AuditLog
        {
            Id = 1,
            EntityName = "Asset",
            EntityId = "AST-1",
            Action = "Verify",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var handler = new GetReportingAuditLogsQueryHandler(db);
        var result = await handler.Handle(new GetReportingAuditLogsQuery(), CancellationToken.None);

        var verifyLog = Assert.Single(result.Logs, l => l.Action.StartsWith("Verify"));
        Assert.Equal("Completed", verifyLog.Status);
    }

    [Fact]
    public async Task ReportingAssetsQuery_FiltersBySearchTerm()
    {
        using var db = CreateContext();

        var productA = new Product { Id = 1, Name = "ThinkPad T14" };
        var productB = new Product { Id = 2, Name = "Dell 27 Monitor" };
        db.Products.AddRange(productA, productB);

        db.Assets.AddRange(
            new Asset
            {
                Id = 1,
                AssetCode = "AST-001",
                AssetTag = "TAG-LAPTOP",
                SerialNumber = "SN-LAPTOP-1",
                AssetDate = DateTime.UtcNow,
                Status = AssetStatus.InUse,
                ProductId = productA.Id,
                Product = productA
            },
            new Asset
            {
                Id = 2,
                AssetCode = "AST-002",
                AssetTag = "TAG-MONITOR",
                SerialNumber = "SN-MONITOR-1",
                AssetDate = DateTime.UtcNow,
                Status = AssetStatus.InUse,
                ProductId = productB.Id,
                Product = productB
            });

        await db.SaveChangesAsync();

        var handler = new GetReportingAssetsQueryHandler(db);

        var byProductName = await handler.Handle(new GetReportingAssetsQuery(SearchTerm: "thinkpad"), CancellationToken.None);
        Assert.Single(byProductName.Assets);
        Assert.Equal("AST-001", byProductName.Assets[0].AssetId);

        var byAssetCode = await handler.Handle(new GetReportingAssetsQuery(SearchTerm: "AST-002"), CancellationToken.None);
        Assert.Single(byAssetCode.Assets);
        Assert.Equal("AST-002", byAssetCode.Assets[0].AssetId);

        var bySerial = await handler.Handle(new GetReportingAssetsQuery(SearchTerm: "sn-laptop-1"), CancellationToken.None);
        Assert.Single(bySerial.Assets);
        Assert.Equal("AST-001", bySerial.Assets[0].AssetId);

        var noMatch = await handler.Handle(new GetReportingAssetsQuery(SearchTerm: "nonexistent"), CancellationToken.None);
        Assert.Empty(noMatch.Assets);
        Assert.Equal(0, noMatch.TotalCount);

        var noFilter = await handler.Handle(new GetReportingAssetsQuery(), CancellationToken.None);
        Assert.Equal(2, noFilter.TotalCount);
    }

    private static TestApplicationDbContext CreateContext() => TestContextFactory.CreateContext();
}
