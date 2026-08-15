using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Assets.Queries;

/// <summary>
/// Query to retrieve all assets that are currently available for checkout.
/// Only returns assets with status InStore and no active reservation.
/// </summary>
public record GetAvailableAssetsForCheckoutQuery : IRequest<List<AvailableCheckoutAssetDto>>;

/// <summary>
/// Lightweight DTO for the checkout form's asset dropdown.
/// Contains only the fields needed to identify and select an asset.
/// </summary>
public class AvailableCheckoutAssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
}

/// <summary>
/// Handler for <see cref="GetAvailableAssetsForCheckoutQuery"/>.
/// Filters assets where Status is InStore, ReservedForUserId is null, and
/// AssignedUserId is null — matching the eligibility check in
/// <see cref="Commands.CheckoutAssetCommandHandler"/> exactly, so every asset
/// offered here can actually be checked out.
/// </summary>
public class GetAvailableAssetsForCheckoutQueryHandler : IRequestHandler<GetAvailableAssetsForCheckoutQuery, List<AvailableCheckoutAssetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableAssetsForCheckoutQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableCheckoutAssetDto>> Handle(GetAvailableAssetsForCheckoutQuery request, CancellationToken cancellationToken)
    {
        return await _context.Assets
            .AsNoTracking()
            .Include(a => a.Product)
            .Include(a => a.Category)
            .Where(a => a.Status == AssetStatus.InStore && a.ReservedForUserId == null && a.AssignedUserId == null)
            .Select(a => new AvailableCheckoutAssetDto
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                ProductName = a.Product.Name,
                CategoryName = a.Category.Name,
                SerialNumber = a.SerialNumber,
            })
            .ToListAsync(cancellationToken);
    }
}
