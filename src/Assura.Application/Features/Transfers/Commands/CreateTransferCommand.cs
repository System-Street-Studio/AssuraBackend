using MediatR;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Transfers.Commands;

public class CreateTransferCommand : IRequest<int>
{
    public int AssetId { get; set; }
    public int AssetRequestId { get; set; }

}
