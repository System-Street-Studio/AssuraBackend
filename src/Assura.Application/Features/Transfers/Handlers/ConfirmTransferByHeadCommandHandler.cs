using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.Transfers.Handlers;

public class ConfirmTransferByHeadCommandHandler : IRequestHandler<Commands.ConfirmTransferByHeadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConfirmTransferByHeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(Commands.ConfirmTransferByHeadCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers.FindAsync(new object[] { request.TransferId }, cancellationToken);

        if (transfer == null)
        {
            throw new Exception($"Transfer {request.TransferId} not found");
        }

        if (transfer.Status != TransferStatus.WaitingForFinalConfirmation)
        {
            throw new Exception($"Cannot confirm transfer in status {transfer.Status}");
        }

        // At this stage the transfer is awaiting the *destination* (ToDivision) head's
        // final confirmation — see GetDivisionHeadTransferQueryHandler's "pending" tab,
        // which scopes the same status by ToDivisionId.
        var caller = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (caller?.DivisionId == null || caller.DivisionId != transfer.ToDivisionId)
        {
            throw new UnauthorizedAccessException("You may only confirm transfers destined for your own division.");
        }

        // Update status to Active or ReadyForHandover
        // According to flow, after confirmation it's either ReadyForHandover or Active.
        transfer.Status = TransferStatus.Active;
        transfer.UpdatedAt = DateTime.UtcNow;
        transfer.ExpectedReturnDate = ParseExpectedReturnDate(transfer.TransferPeriod);

        // Actually move the asset to its new holder — previously only the Transfer
        // row's status changed, leaving Asset.AssignedUserId/Status pointing at the
        // original holder for the entire duration of the transfer.
        var asset = await _context.Assets.FindAsync(new object[] { transfer.AssetId }, cancellationToken);
        if (asset != null)
        {
            if (asset.Status == AssetStatus.UnderMaintenance)
            {
                throw new InvalidOperationException($"Asset {asset.AssetCode} is currently under maintenance and cannot be transferred.");
            }
            asset.AssignedUserId = transfer.TargetUserId;
            asset.Status = AssetStatus.Transferred;
            asset.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // TransferPeriod is free text of the form "<start> to <end>" (see
    // CreateTransferCommandHandler.ExtractTransferPeriod / transfer-form.ts), built from
    // the browser's locale-formatted date string. Best-effort parse of the end date; if
    // it can't be parsed, ExpectedReturnDate is left null and the transfer simply won't
    // be flagged overdue, matching today's behavior for periods with no end date.
    // Accepts " to " as well as a bare "-" separator, and falls back to the last
    // whitespace-delimited token (the end date) when neither separator is present, so
    // stray whitespace or a different phrasing doesn't silently drop the return date.
    private static readonly string[] PeriodSeparators = { " to ", " - ", "-" };

    private static DateTime? ParseExpectedReturnDate(string? transferPeriod)
    {
        if (string.IsNullOrWhiteSpace(transferPeriod))
            return null;

        var trimmed = transferPeriod.Trim();

        foreach (var separator in PeriodSeparators)
        {
            var parts = trimmed.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && DateTime.TryParse(parts[1], out var endDate))
                return endDate;
        }

        // Single date with no separator at all — treat it as the end date itself.
        return DateTime.TryParse(trimmed, out var singleDate) ? singleDate : null;
    }
}
