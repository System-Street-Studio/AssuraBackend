using MediatR;

namespace Assura.Application.Features.Transfers.Commands;

public class CreateTransferCommand : IRequest<int>
{
    public int AssetId { get; set; }
    public int? AssetRequestId { get; set; }
    public int? UserId { get; set; }
}
