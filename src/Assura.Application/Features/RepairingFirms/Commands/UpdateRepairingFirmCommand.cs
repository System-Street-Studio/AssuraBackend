using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.RepairingFirms.Commands;

public record UpdateRepairingFirmCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class UpdateRepairingFirmCommandHandler : IRequestHandler<UpdateRepairingFirmCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateRepairingFirmCommandHandler> _logger;

    public UpdateRepairingFirmCommandHandler(IApplicationDbContext context, ILogger<UpdateRepairingFirmCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateRepairingFirmCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DEBUG] UpdateRepairingFirmCommandHandler: Updating repairing firm {Id}: {@Request}", request.Id, request);

        var entity = await _context.RepairingFirms
            .FirstOrDefaultAsync(rf => rf.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("[DEBUG] UpdateRepairingFirmCommandHandler: Repairing firm {Id} not found", request.Id);
            return false;
        }

        entity.Name = request.Name;
        entity.ContactPerson = request.ContactPerson;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.Address = request.Address;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[DEBUG] UpdateRepairingFirmCommandHandler: Updated firm with ID {Id}", entity.Id);
        return true;
    }
}
