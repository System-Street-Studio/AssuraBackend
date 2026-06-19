using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using MediatR;

namespace Assura.Application.Features.Reporting.Commands;

public class CreateCustomReportCommand : IRequest<string>
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsScheduled { get; set; }
    public string? ScheduleFrequency { get; set; }
}

public class CreateCustomReportCommandHandler : IRequestHandler<CreateCustomReportCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCustomReportCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(CreateCustomReportCommand request, CancellationToken cancellationToken)
    {
        var reportCount = _context.CustomReports.Count();
        
        var nextId = $"RPT-{DateTime.UtcNow:yyyyMM}-C{(reportCount + 1).ToString().PadLeft(3, '0')}";

        var report = new CustomReport
        {
            ReportIdCode = nextId,
            Title = request.Title,
            Type = request.Type,
            Owner = _currentUserService.UserId ?? "System",
            Period = "Just now",
            Status = request.IsScheduled ? "Scheduled" : "Pending",
            Size = "0 KB",
            IsScheduled = request.IsScheduled,
            ScheduleFrequency = request.ScheduleFrequency
        };

        if (request.IsScheduled)
        {
            report.NextRunDate = request.ScheduleFrequency switch
            {
                "Daily" => DateTime.UtcNow.AddDays(1),
                "Weekly" => DateTime.UtcNow.AddDays(7),
                "Monthly" => DateTime.UtcNow.AddMonths(1),
                _ => DateTime.UtcNow.AddDays(1)
            };
        }

        _context.CustomReports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        return nextId;
    }
}
