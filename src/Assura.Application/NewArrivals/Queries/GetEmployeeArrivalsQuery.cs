using Assura.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.NewArrivals.Queries;

public record GetEmployeeArrivalsQuery(int UserId, int? DivisionId = null) : IRequest<List<AssetInformingDto>>;

public class GetEmployeeArrivalsQueryHandler : IRequestHandler<GetEmployeeArrivalsQuery, List<AssetInformingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeArrivalsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetInformingDto>> Handle(GetEmployeeArrivalsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AssetInformings
            .AsNoTracking()
            .Include(x => x.Division)
            .Include(x => x.TargetEmployee)
            .Where(x => !x.IsDeleted && (
                x.TargetEmployeeId == request.UserId ||
                (x.TargetEmployeeId == null && request.DivisionId.HasValue && x.DivisionId == request.DivisionId.Value)
            ));

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AssetInformingDto
            {
                Id = x.Id,
                ItemName = x.ItemName,
                Model = x.Model,
                Warranty = x.Warranty,
                Quantity = x.Quantity,
                PurchasedDate = x.PurchasedDate,
                PurchasedPrice = x.PurchasedPrice,
                Status = x.Status,
                DivisionId = x.DivisionId,
                DivisionName = x.Division != null ? x.Division.Name : string.Empty,
                TargetEmployeeId = x.TargetEmployeeId,
                TargetEmployeeName = x.TargetEmployee != null ? (x.TargetEmployee.FirstName + " " + x.TargetEmployee.LastName).Trim() : null,
                Remarks = x.Remarks,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
