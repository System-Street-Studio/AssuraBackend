using Assura.Domain.Common;
using Assura.Domain.Enums;

namespace Assura.Domain.Entities;

public class Transfer : BaseEntity

{

    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    // Due date the asset is expected back by, parsed from TransferPeriod when the
    // transfer is confirmed Active. Distinct from ReturnDate, which instead records
    // when the asset was actually returned (set by ReturnActiveTransferCommand) —
    // TransferOverdueCheckerService compares against this field, not ReturnDate.
    public DateTime? ExpectedReturnDate { get; set; }
    public string? Reason { get; set; }

    public string? TransferPeriod { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.PendingOwnerApproval;

    public int AssetRequestId { get; set; }
    public AssetRequest AssetRequest { get; set; } = null!;

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int FromDivisionId { get; set; }
    public Division FromDivision { get; set; }= null!;

    public int ToDivisionId { get; set; }
    public Division ToDivision { get; set; }= null!;

    public int? TransferById { get; set; }
    public User? TransferBy { get; set; } 

    public int TargetUserId { get; set; }
    public User TargetUser { get; set; } = null!;
    
    public int CurrentHolderId { get; set; }
    public User CurrentHolder { get; set; }= null!;

}

