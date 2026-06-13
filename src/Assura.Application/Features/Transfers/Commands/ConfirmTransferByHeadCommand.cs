using MediatR;

namespace Assura.Application.Features.Transfers.Commands;

public record ConfirmTransferByHeadCommand(int TransferId, int DivisionHeadId) : IRequest<bool>;
