using MediatR;

namespace Assura.Application.Features.Transfers.Commands;

// The handler for this command lives in Features/Transfers/Handlers/
// ApproveTransferByHeadCommandHandler.cs. A second, near-duplicate handler used to
// live in this file too; MediatR's assembly scan silently registered only one of the
// two (the Handlers/ one), leaving this file's implementation as dead, never-invoked
// code — but it wrote a TransferApproval audit row on approval that the live handler
// didn't. Removed the dead duplicate here; the audit-row behavior still needs to be
// ported into the live handler as a follow-up (tracked separately, out of scope for
// the division-scoping IDOR fix this change makes).
public record ApproveTransferByHeadCommand(int TransferId, int UserId) : IRequest<bool>;
