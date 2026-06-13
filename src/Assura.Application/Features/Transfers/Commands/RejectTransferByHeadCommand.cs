using MediatR;

namespace Assura.Application.Features.Transfers.Commands;

public record RejectTransferByHeadCommand(int TransferId, int DivisionHeadId, string Reason) : IRequest<bool>;
