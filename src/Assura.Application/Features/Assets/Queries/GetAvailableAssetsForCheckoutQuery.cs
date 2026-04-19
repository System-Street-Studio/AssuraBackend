using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Assets.Queries;

public record GetAvailableAssetsForCheckoutQuery : IRequest<List<AvailableCheckoutAssetDto>>;

public class AvailableCheckoutAssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
}

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
            .Where(a => a.Status == AssetStatus.InStore && a.AssignedUserId == null)
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
