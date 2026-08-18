using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.SystemAdmin.Queries;

public record GetSystemLogsQuery() : IRequest<List<SystemAdminAuditLogDto>>;

public class GetSystemLogsQueryHandler : IRequestHandler<GetSystemLogsQuery, List<SystemAdminAuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSystemLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SystemAdminAuditLogDto>> Handle(GetSystemLogsQuery request, CancellationToken cancellationToken)
    {
        // Filter out normal audit trails, keep only Errors and Exceptions for Troubleshooting
        return await _context.AuditLogs
            .Where(a => a.Action.Contains("Error") || a.Action.Contains("Exception"))
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .Select(a => new SystemAdminAuditLogDto
            {
                Id = a.Id,
                EntityName = a.EntityName,
                Action = a.Action,
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedBy,
                OldValues = a.OldValues,
                NewValues = a.NewValues
            })
            .ToListAsync(cancellationToken);
    }
}
