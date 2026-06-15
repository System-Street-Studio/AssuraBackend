namespace Assura.Domain.Enums;

public enum TransferStatus
{
    PendingOwnerApproval = 1,  // Current Employee (Asset Holder) needs to accept

    PendingOwnerDivisionHeadApproval = 2,  // Division Head of current holder needs to approve

    WaitingForFinalConfirmation = 3,  // Previous Division Head confirmation

    Active = 4,      // Active Transfers 

    Completed = 5,   // after returning the asset

    Rejected = 6,

    Cancelled = 7
}
