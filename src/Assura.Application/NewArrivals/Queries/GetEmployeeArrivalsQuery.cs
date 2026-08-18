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
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        var effectiveDivisionId = request.DivisionId ?? user?.DivisionId;

        var query = _context.AssetInformings
            .AsNoTracking()
            .Include(x => x.Division)
            .Include(x => x.TargetEmployee)
            .Where(x => !x.IsDeleted && (
                x.TargetEmployeeId == request.UserId ||
                (x.TargetEmployeeId == null && effectiveDivisionId.HasValue && x.DivisionId == effectiveDivisionId.Value) ||
                (user != null && user.Role == Assura.Domain.Enums.UserRole.DivisionHead && effectiveDivisionId.HasValue && x.DivisionId == effectiveDivisionId.Value)
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
