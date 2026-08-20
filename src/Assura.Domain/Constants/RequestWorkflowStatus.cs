namespace Assura.Domain.Constants;

public static class RequestWorkflowStatus
{
    public const string PendingDivisionHeadApproval = "PendingDivisionHeadApproval";
    public const string PendingStorekeeperReview = "PendingStorekeeperReview";
    public const string TemporaryAssigned = "TemporaryAssigned";
    public const string PendingProcurement = "PendingProcurement";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    // Not part of the Division-Head/Storekeeper review chain above — this is the terminal
    // status for a checked-out asset, written by both ConfirmTemporaryAssignmentCommand (the
    // request-driven handover) and CheckoutAssetCommand (a separate, direct ad-hoc checkout of
    // unreserved stock — see its own doc comment). Named as a shared constant so both stay in
    // sync instead of relying on two independent string literals matching by coincidence.
    public const string CheckedOut = "Checked Out";
}
