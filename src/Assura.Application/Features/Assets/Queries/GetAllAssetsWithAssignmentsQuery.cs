using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Assets.DTOs;

namespace Assura.Application.Features.Assets.Queries;

public record GetAllAssetsWithAssignmentsQuery : IRequest<List<AssetWithAssignmentDto>>;

public class GetAllAssetsWithAssignmentsQueryHandler : IRequestHandler<GetAllAssetsWithAssignmentsQuery, List<AssetWithAssignmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllAssetsWithAssignmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetWithAssignmentDto>> Handle(GetAllAssetsWithAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var assets = await _context.Assets
            .Include(x => x.Product)
            .Include(x => x.Division)
            .Include(x => x.AssignedUser)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.AssetDate)
            .ToListAsync(cancellationToken);

        return assets.Select(asset => new AssetWithAssignmentDto
        {
            Id = asset.Id,
            AssetTag = asset.AssetTag ?? "N/A",
            AssetCode = asset.AssetCode,
            ProductName = asset.Product?.Name ?? "Unknown Product",
            DivisionName = asset.Division?.Name ?? "Unknown Division",
            Status = asset.Status,
            AssignedUserName = asset.AssignedUser != null 
                ? $"{asset.AssignedUser.FirstName} {asset.AssignedUser.LastName}" 
                : null,
            AssignedUserEmail = asset.AssignedUser?.Email,
            AssignedUserId = asset.AssignedUserId,
            SerialNumber = asset.SerialNumber,
            PurchaseValue = asset.PurchaseValue,
            AssetDate = asset.AssetDate,
            Notes = asset.Notes,
            QrCode = asset.QrCode
        }).ToList();
    }
}
