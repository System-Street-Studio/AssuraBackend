using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Maintenances.Commands;

public record UpdateMaintenanceStatusCommand(int Id, string Status) : IRequest<bool>;

public class UpdateMaintenanceStatusCommandHandler : IRequestHandler<UpdateMaintenanceStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateMaintenanceStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateMaintenanceStatusCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _context.Maintenances.FindAsync(new object[] { request.Id }, cancellationToken);
        if (maintenance == null) return false;

        maintenance.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
