using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Reporting.Commands;

public record MarkReportCompletedCommand(string ReportId) : IRequest<bool>;

public class MarkReportCompletedCommandHandler : IRequestHandler<MarkReportCompletedCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public MarkReportCompletedCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(MarkReportCompletedCommand request, CancellationToken cancellationToken)
    {
        var report = await _context.CustomReports
            .FirstOrDefaultAsync(r => r.ReportIdCode == request.ReportId && !r.IsDeleted, cancellationToken);

        if (report is null)
        {
            return false;
        }

        report.Status = "Completed";
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
