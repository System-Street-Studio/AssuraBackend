using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Buyers.Commands.Create;

public record CreateBuyerCommand(
    string Name,
    string Contact,
    string Email,
    string Phone,
    string Category
) : IRequest<int>;

public class CreateBuyerCommandHandler : IRequestHandler<CreateBuyerCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateBuyerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateBuyerCommand request, CancellationToken cancellationToken)
    {
        var buyer = new Buyer
        {
            Name = request.Name,
            Contact = request.Contact,
            Email = request.Email,
            Phone = request.Phone,
            Category = request.Category,
            Status = BuyerStatus.Active
        };

        _context.Buyers.Add(buyer);
        await _context.SaveChangesAsync(cancellationToken);

        return buyer.Id;
    }
}
