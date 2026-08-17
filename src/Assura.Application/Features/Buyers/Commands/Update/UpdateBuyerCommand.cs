using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assura.Application.Features.Buyers.Commands.Update;

public record UpdateBuyerCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Status { get; set; }

    public UpdateBuyerCommand() { }

    public UpdateBuyerCommand(int id, string name, string contact, string email, string phone, string category, string? status)
    {
        Id = id;
        Name = name;
        Contact = contact;
        Email = email;
        Phone = phone;
        Category = category;
        Status = status;
    }
}

public class UpdateBuyerCommandHandler : IRequestHandler<UpdateBuyerCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateBuyerCommandHandler> _logger;

    public UpdateBuyerCommandHandler(IApplicationDbContext context, ILogger<UpdateBuyerCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateBuyerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DEBUG] UpdateBuyerCommandHandler: Updating buyer {Id}: {@Request}", request.Id, request);

        var buyer = await _context.Buyers
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (buyer == null)
        {
            _logger.LogWarning("[DEBUG] UpdateBuyerCommandHandler: Buyer {Id} not found", request.Id);
            return false;
        }

        buyer.Name = request.Name;
        buyer.Contact = request.Contact;
        buyer.Email = request.Email;
        buyer.Phone = request.Phone;
        buyer.Category = request.Category;

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<BuyerStatus>(request.Status, true, out var status))
        {
            buyer.Status = status;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("[DEBUG] UpdateBuyerCommandHandler: Successfully updated buyer {Id}", buyer.Id);
        return true;
    }
}
