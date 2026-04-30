using MediatR;
using Assura.Application.Features.Transfers.DTOs;

namespace Assura.Application.Features.Transfers.Queries;

public class GetAllTransfersQuery : IRequest<List<TransferDto>>
{
 
    public int? DivisionId { get; set; }
    public string? Status { get; set; }
    public int? AssetId { get; set; }
    public int? CurrentHolderId { get; set; }
    public int? DivisionHeadUserId { get; set; }
}


