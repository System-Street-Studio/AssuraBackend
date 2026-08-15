using MediatR;

namespace Assura.Application.Features.Transfers.Commands;

// The handler for this command lives in Features/Transfers/Handlers/
// ConfirmTransferByHeadCommandHandler.cs. A second, near-duplicate handler used to
// live in this file too; MediatR's assembly scan silently registered only one of the
// two (the Handlers/ one), leaving this file's implementation as dead, never-invoked
// code — but it also updated the Asset's Status to Transferred and wrote a
// TransferApproval audit row, neither of which the live handler does. Removed the
// dead duplicate here; whether the asset-status update and audit row are actually
// needed on confirm is a follow-up decision (tracked separately, out of scope for the
// division-scoping IDOR fix this change makes).
public record ConfirmTransferByHeadCommand(int TransferId, int UserId) : IRequest<bool>;
