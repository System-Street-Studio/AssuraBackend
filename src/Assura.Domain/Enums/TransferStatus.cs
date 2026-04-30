namespace Assura.Domain.Enums;

public enum TransferStatus
{
    PendingOwnerApproval = 1,  // Current Employee (Asset Holder) needs to approve

    PendingOwnerDivisionHeadApproval = 2,  // Division Head of current holder needs to approve

    WaitingForFinalConfirmation = 3,  // Previous Division Head confirmation

    //ReadyForHandover = 4,  // Ready for asset handover

    Active = 4,      // Active Transfers 

    Completed = 5,   // after returning the asset

    Rejected = 6,

    Cancelled = 7
}
