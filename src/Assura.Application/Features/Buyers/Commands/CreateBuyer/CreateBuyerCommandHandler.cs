using MediatR;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using Assura.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Features.Buyers.Commands.CreateBuyer;

public class CreateBuyerCommandHandler : IRequestHandler<CreateBuyerCommand, string>
{
    private readonly IApplicationDbContext _context;

    public CreateBuyerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(CreateBuyerCommand request, CancellationToken cancellationToken)
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

        return buyer.Id.ToString();
    }
}
