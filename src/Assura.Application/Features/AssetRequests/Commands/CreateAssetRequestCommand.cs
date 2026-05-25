using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Assura.Application.Features.AssetRequests.Commands;


public class AttachmentUploadModel
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
}

public record CreateAssetRequestCommand : IRequest<int>
{
    public required string EmployeeId { get; set; }
    public required string SubmittedBy { get; set; }
    public string AssetCategory { get; set; } = string.Empty;
    public required string AssetName { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public required string Priority { get; set; }
    public required string RequestType { get; set; }
    public DateTime SubmittedDate { get; set; } = DateTime.Now;

    
    public List<AttachmentUploadModel> UploadedAttachments { get; set; } = new();
}

public class CreateAssetRequestHandler : IRequestHandler<CreateAssetRequestCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateAssetRequestHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(CreateAssetRequestCommand request, CancellationToken cancellationToken)
    {
        int? userId = int.TryParse(request.EmployeeId, out var id) ? id : null;
        int? divisionId = null;

        if (userId.HasValue)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
            divisionId = user?.DivisionId;  
        }

        var entity = new AssetRequest
        {
            RequesterId = request.EmployeeId,      
            RequesterName = request.SubmittedBy,    
            SubmittedDate = request.SubmittedDate,  
            AssetCategory = request.AssetCategory,
            AssetName = request.AssetName,
            Description = request.Description,
            Reason = request.Reason,
            Quantity = request.Quantity,
            Priority = request.Priority,
            RequestType = request.RequestType,
            Status = Domain.Enums.RequestStatus.Pending,
            UserId = userId,
            DivisionId = divisionId,
            Attachments = new List<AssetAttachment>() 
        };

      
        foreach (var fileDto in request.UploadedAttachments)
        {
            entity.Attachments.Add(new AssetAttachment
            {
                FileName = fileDto.FileName,
                FileUrl = fileDto.FileUrl,
                FileSize = fileDto.FileSize,
                FileType = fileDto.FileType,
                UploadedDate = DateTime.UtcNow
            });
        }

        _context.AssetRequests.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Notify division heads 
        if (divisionId.HasValue)
        {
            var divisionHeads = await _context.Users
                .Where(u => u.DivisionId == divisionId.Value && u.Role == UserRole.DivisionHead)
                .ToListAsync(cancellationToken);

            string title;
            string message;
            var normalizedRequestType = entity.RequestType?.ToLower().Replace(" ", "") ?? "";

            switch (normalizedRequestType)
            {
                case "newasset":
                    title = "New Asset Request";
                    message = $"A new asset request for '{entity.AssetName}' has been submitted and requires your review.";
                    break;
                case "transfer":
                    title = "Asset Transfer Request";
                    message = $"A transfer request for asset '{entity.AssetName}' has been submitted and requires your review.";
                    break;
                case "maintenance":
                    title = "Asset Maintenance Request";
                    message = $"A maintenance request for asset '{entity.AssetName}' has been submitted and requires your review.";
                    break;
                case "discard":
                    title = "Asset Discard Request";
                    message = $"A discard request for asset '{entity.AssetName}' has been submitted and requires your review.";
                    break;
                default:
                    title = "New Asset Request Submitted";
                    message = $"A new asset request ({entity.AssetName}) has been submitted and requires your review.";
                    break;
            }

            foreach (var head in divisionHeads)
            {
                _context.Notifications.Add(new Notification
                {
                    Title = title,
                    Message = message,
                    UserId = head.Id,
                    Type = "Info",
                    ReferenceId = entity.Id.ToString()
                });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
        return entity.Id;
    }
}