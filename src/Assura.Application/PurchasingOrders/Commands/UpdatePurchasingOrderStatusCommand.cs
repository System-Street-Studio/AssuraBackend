using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
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

        var wasAlreadyRegistered = po.Status == "Registered";
        po.Status = request.Status;
        po.UpdatedAt = DateTime.UtcNow;

        // The moment a PO is (first) marked Registered, the physical asset for it exists in
        // inventory (registered via the GRN "New GRN" -> asset-form -> PO auto-fill flow, which
        // stamps the new Asset.PurchasingOrderId). Close the loop: find that asset and whichever
        // request was actually waiting on this PO, and finish the handover — otherwise a
        // procurement-sourced request sits at "Approved" forever with no asset ever assigned to
        // the person who originally asked for it.
        if (request.Status == "Registered" && !wasAlreadyRegistered)
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.PurchasingOrderId == po.Id && a.AssignedUserId == null, cancellationToken);

            if (asset != null)
            {
                var assetReq = await _context.AssetRequests
                    .FirstOrDefaultAsync(r => r.PurchasingOrderId == po.Id && r.Status == RequestStatus.Approved && r.AssetId == null, cancellationToken);
                var req = assetReq == null
                    ? await _context.Requests.FirstOrDefaultAsync(r => r.PurchasingOrderId == po.Id && r.Status == Assura.Domain.Constants.RequestWorkflowStatus.Approved && r.AssetId == null, cancellationToken)
                    : null;

                int? requesterId = null;
                if (assetReq != null)
                {
                    assetReq.AssetId = asset.Id;
                    assetReq.Status = RequestStatus.Completed;
                    requesterId = assetReq.UserId ?? (int.TryParse(assetReq.RequesterId, out var rid) ? rid : null);
                }
                else if (req != null)
                {
                    req.AssetId = asset.Id;
                    req.Status = Assura.Domain.Constants.RequestWorkflowStatus.Approved; // already Approved; kept for clarity, AssetId is the completion signal here
                    requesterId = req.RequesterId;
                }

                if (requesterId.HasValue)
                {
                    asset.AssignedUserId = requesterId;
                    asset.Status = AssetStatus.InUse;

                    _context.Notifications.Add(new Notification
                    {
                        Title = "Asset Ready",
                        Message = $"The asset you requested has arrived and been assigned to you ({asset.AssetCode}).",
                        UserId = requesterId.Value,
                        Type = "Success",
                        ReferenceId = asset.Id.ToString()
                    });
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
