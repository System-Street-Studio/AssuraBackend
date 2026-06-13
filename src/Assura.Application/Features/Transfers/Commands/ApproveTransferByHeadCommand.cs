using MediatR;

namespace Assura.Application.Features.Transfers.Commands;

public record ApproveTransferByHeadCommand(int TransferId, int DivisionHeadId) : IRequest<bool>;
