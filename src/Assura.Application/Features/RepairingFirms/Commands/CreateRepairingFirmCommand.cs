using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.RepairingFirms.Commands;

public record CreateRepairingFirmCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class CreateRepairingFirmCommandHandler : IRequestHandler<CreateRepairingFirmCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateRepairingFirmCommandHandler> _logger;

    public CreateRepairingFirmCommandHandler(IApplicationDbContext context, ILogger<CreateRepairingFirmCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> Handle(CreateRepairingFirmCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DEBUG] CreateRepairingFirmCommandHandler: Creating new repairing firm: {@Request}", request);

        var firm = new RepairingFirm
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address
        };

        _context.RepairingFirms.Add(firm);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[DEBUG] CreateRepairingFirmCommandHandler: Created firm with ID {Id}", firm.Id);
        return firm.Id;
    }
}
