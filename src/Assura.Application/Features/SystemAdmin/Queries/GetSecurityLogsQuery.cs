using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.SystemAdmin.Queries;

public record GetSecurityLogsQuery() : IRequest<List<SystemAdminAuditLogDto>>;

public class GetSecurityLogsQueryHandler : IRequestHandler<GetSecurityLogsQuery, List<SystemAdminAuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSecurityLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SystemAdminAuditLogDto>> Handle(GetSecurityLogsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(100) // Limit to the last 100 logs to prevent massive payload
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
