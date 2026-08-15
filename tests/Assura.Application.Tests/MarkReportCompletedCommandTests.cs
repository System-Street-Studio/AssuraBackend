using Assura.Application.Features.Reporting.Commands;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;

namespace Assura.Application.Tests;

public class MarkReportCompletedCommandTests
{
    [Fact]
    public async Task Handle_WithExistingPendingReport_MarksItCompletedAndReturnsTrue()
    {
        using var db = TestContextFactory.CreateContext();

        db.CustomReports.Add(new CustomReport
        {
            ReportIdCode = "RPT-202608-C001",
            Title = "Division Variance Register",
            Type = "Exception",
            Owner = "1",
            Period = "Aug 2026",
            Status = "Pending",
            Size = "0 KB"
        });
        await db.SaveChangesAsync();

        var handler = new MarkReportCompletedCommandHandler(db);
        var result = await handler.Handle(new MarkReportCompletedCommand("RPT-202608-C001"), CancellationToken.None);

        Assert.True(result);
        var updated = db.CustomReports.Single(r => r.ReportIdCode == "RPT-202608-C001");
        Assert.Equal("Completed", updated.Status);
    }

    [Fact]
    public async Task Handle_WithNonExistentReportId_ReturnsFalse()
    {
        using var db = TestContextFactory.CreateContext();

        var handler = new MarkReportCompletedCommandHandler(db);
        var result = await handler.Handle(new MarkReportCompletedCommand("RPT-202608-001"), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task Handle_WithSoftDeletedReport_ReturnsFalse()
    {
        using var db = TestContextFactory.CreateContext();

        db.CustomReports.Add(new CustomReport
        {
            ReportIdCode = "RPT-202608-C002",
            Title = "Old Report",
            Type = "Audit",
            Owner = "1",
            Period = "Aug 2026",
            Status = "Pending",
            Size = "0 KB",
            IsDeleted = true
        });
        await db.SaveChangesAsync();

        var handler = new MarkReportCompletedCommandHandler(db);
        var result = await handler.Handle(new MarkReportCompletedCommand("RPT-202608-C002"), CancellationToken.None);

        Assert.False(result);
    }
}
