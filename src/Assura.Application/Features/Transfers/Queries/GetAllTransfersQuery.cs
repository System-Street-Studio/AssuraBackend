using MediatR;
using Assura.Application.Features.Transfers.DTOs;

namespace Assura.Application.Features.Transfers.Queries;

public class GetAllTransfersQuery : IRequest<List<TransferDto>>
{
 
    public int? DivisionId { get; set; }
    public string? Status { get; set; }
    public int? AssetId { get; set; }
    public int? CurrentHolderId { get; set; }
}

public class TransferDto
{
    public int Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AssetRequestId { get; set; }
    public int AssetId { get; set; }
    public string? AssetTag { get; set; }
    public string? AssetName { get; set; }
    public int? FromDivisionId { get; set; }
    public string? FromDivisionName { get; set; }
    public int? ToDivisionId { get; set; }
    public string? ToDivisionName { get; set; }
    public int? TransferById { get; set; }
    public string? TransferByName { get; set; }
    public int? TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public int? CurrentHolderId { get; set; }
    public string? CurrentHolderName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
