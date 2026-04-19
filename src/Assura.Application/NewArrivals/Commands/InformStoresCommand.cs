using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.NewArrivals.Commands;

public record InformStoresCommand(InformStoresDto InformStoresDto) : IRequest<int>;

public class InformStoresCommandHandler : IRequestHandler<InformStoresCommand, int>
{
    private readonly IApplicationDbContext _context;

    public InformStoresCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(InformStoresCommand request, CancellationToken cancellationToken)
    {
        var dto = request.InformStoresDto;

        var entity = new AssetInforming
        {
            ItemName = dto.ItemName,
            Model = dto.Model,
            Warranty = dto.Warranty,
            Quantity = dto.Quantity,
            PurchasedDate = dto.PurchasedDate,
            PurchasedPrice = dto.PurchasedPrice,
            DivisionId = dto.DivisionId,
            Status = "Pending"
        };

        _context.AssetInformings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Notify Storekeepers
        var storekeepers = await _context.Users
            .Where(u => u.Role == UserRole.Storekeeper)
            .ToListAsync(cancellationToken);

        foreach (var storekeeper in storekeepers)
        {
            _context.Notifications.Add(new Notification
            {
                Title = "New Item Received",
                Message = $"An order for '{dto.ItemName}' has been received and informed by Procurement.",
                UserId = storekeeper.Id,
                Type = "Info",
                ReferenceId = entity.Id.ToString()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
