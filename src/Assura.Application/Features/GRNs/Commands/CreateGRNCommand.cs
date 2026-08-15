using Assura.Application.Common.Interfaces;
using Assura.Application.Features.GRNs.Queries;
using Assura.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.GRNs.Commands;

/// <summary>
/// Command to record a Goods Received Note for an asset delivered against a
/// purchasing order. This is the Storekeeper's record of "goods physically
/// arrived and were checked in" — distinct from the asset's own creation.
/// </summary>
public record CreateGRNCommand(int PurchasingOrderId, int AssetId, DateTime ReceivedDate, string? ReceivedBy, string? Notes) : IRequest<GRNDto>;

public class CreateGRNCommandValidator : AbstractValidator<CreateGRNCommand>
{
    public CreateGRNCommandValidator()
    {
        RuleFor(x => x.PurchasingOrderId).GreaterThan(0);
        RuleFor(x => x.AssetId).GreaterThan(0);
        RuleFor(x => x.ReceivedDate)
            .Must(d => d <= DateTime.UtcNow.AddDays(1))
            .WithMessage("Received date cannot be in the future.");
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.ReceivedBy).MaximumLength(200);
    }
}

public class CreateGRNCommandHandler : IRequestHandler<CreateGRNCommand, GRNDto>
{
    private readonly IApplicationDbContext _context;

    public CreateGRNCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GRNDto> Handle(CreateGRNCommand request, CancellationToken cancellationToken)
    {
        var purchasingOrder = await _context.PurchasingOrders
            .FirstOrDefaultAsync(po => po.Id == request.PurchasingOrderId, cancellationToken);
        if (purchasingOrder == null)
        {
            throw new ValidationException("Purchasing order not found.");
        }

        var asset = await _context.Assets
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);
        if (asset == null)
        {
            throw new ValidationException("Asset not found.");
        }

        var alreadyReceived = await _context.GRNs
            .AnyAsync(g => g.AssetId == request.AssetId, cancellationToken);
        if (alreadyReceived)
        {
            throw new ValidationException("A GRN has already been recorded for this asset.");
        }

        var grnNumber = $"GRN-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var grn = new GRN
        {
            GrnNumber = grnNumber,
            ReceivedDate = request.ReceivedDate,
            ReceivedBy = string.IsNullOrWhiteSpace(request.ReceivedBy) ? "Storekeeper" : request.ReceivedBy.Trim(),
            Notes = request.Notes,
            PurchasingOrderId = request.PurchasingOrderId,
            AssetId = request.AssetId,
        };

        _context.GRNs.Add(grn);
        await _context.SaveChangesAsync(cancellationToken);

        return new GRNDto
        {
            Id = grn.Id,
            GrnNumber = grn.GrnNumber,
            ReceivedDate = grn.ReceivedDate,
            ReceivedBy = grn.ReceivedBy,
            Notes = grn.Notes,
            PurchasingOrderId = purchasingOrder.Id,
            PurchasingOrderNumber = purchasingOrder.OrderNumber,
            SupplierName = (await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == purchasingOrder.SupplierId, cancellationToken))?.Name ?? "-",
            AssetId = asset.Id,
            AssetCode = asset.AssetCode,
            ProductName = asset.Product?.Name ?? "-",
            CreatedAt = grn.CreatedAt,
        };
    }
}
