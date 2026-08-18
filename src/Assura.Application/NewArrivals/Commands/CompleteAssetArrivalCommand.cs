using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.NewArrivals.Commands;

public record CompleteAssetArrivalCommand(int InformingId, string? Remarks = null) : IRequest<bool>;

public class CompleteAssetArrivalCommandHandler : IRequestHandler<CompleteAssetArrivalCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CompleteAssetArrivalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CompleteAssetArrivalCommand request, CancellationToken cancellationToken)
    {
        var informing = await _context.AssetInformings
            .FirstOrDefaultAsync(a => a.Id == request.InformingId, cancellationToken);

        if (informing == null)
            throw new Exception("Asset arrival record not found.");

        informing.Status = "Completed";
        informing.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(request.Remarks))
        {
            informing.Remarks = request.Remarks;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
