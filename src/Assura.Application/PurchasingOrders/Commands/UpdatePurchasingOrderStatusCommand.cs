using Assura.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.PurchasingOrders.Commands;

public record UpdatePurchasingOrderStatusCommand(int Id, string Status = "Registered") : IRequest<bool>;

public class UpdatePurchasingOrderStatusCommandHandler : IRequestHandler<UpdatePurchasingOrderStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdatePurchasingOrderStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdatePurchasingOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchasingOrders.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po == null) return false;

        po.Status = request.Status;
        po.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
