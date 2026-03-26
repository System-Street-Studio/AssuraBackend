using MediatR;
namespace Assura.Application.Features.AssetRequests.Events;

public record AssetRequestApprovedEvent(
    int Id, 
    string AssetName,
    string AssetCategory,
    int Quantity,
    string RequestType,
    string Priority,
    string Status,
    string RequesterName,
    string RequesterId,
    string Attachments,
    DateTime SubmittedDate,
    string Description,
    string Reason
     //string ApprovalBy 

) : INotification;