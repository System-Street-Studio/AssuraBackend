using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
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
    public int? AssetId { get; set; }

    public List<AttachmentUploadModel> UploadedAttachments { get; set; } = new();
}

public class CreateAssetRequestCommandValidator : AbstractValidator<CreateAssetRequestCommand>
{
    public CreateAssetRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.SubmittedBy).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AssetName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AssetCategory).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Reason).MaximumLength(2000);
        RuleFor(x => x.Priority).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RequestType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AssetId).GreaterThan(0).When(x => x.AssetId.HasValue);

        // Discard specifically requires a reason to be given — the frontend already
        // marks this field required for a discard request, but only client-side.
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required for a discard request.")
            .When(x => string.Equals(x.RequestType, "Discard", StringComparison.OrdinalIgnoreCase));
    }
}

public class CreateAssetRequestHandler : IRequestHandler<CreateAssetRequestCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateAssetRequestHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(CreateAssetRequestCommand request, CancellationToken cancellationToken)
    {
        int? userId = int.TryParse(request.EmployeeId, out var id) ? id : null;
        int? divisionId = null;
        string requesterName = request.SubmittedBy;

        if (userId.HasValue)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
            divisionId = user?.DivisionId;

            // Prefer the authenticated user's own name over the client-supplied
            // SubmittedBy value, which is not trustworthy identity information.
            if (user != null)
            {
                requesterName = $"{user.FirstName} {user.LastName}";
            }
        }

        // A discard request must target an asset actually assigned to the requester —
        // without this, any employee could get any other employee's (or division's)
        // asset discarded by simply supplying its AssetId.
        if (string.Equals(request.RequestType, "Discard", StringComparison.OrdinalIgnoreCase) && request.AssetId.HasValue)
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.Id == request.AssetId.Value, cancellationToken);

            if (asset == null || !userId.HasValue || asset.AssignedUserId != userId.Value)
            {
                throw new FluentValidation.ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.AssetId), "You can only request a discard for an asset assigned to you.")
                });
            }
        }

        var entity = new AssetRequest
        {
            RequesterId = request.EmployeeId,
            RequesterName = requesterName,
            SubmittedDate = request.SubmittedDate,  
            AssetCategory = request.AssetCategory,
            AssetName = request.AssetName,
            Description = request.Description,
            Reason = request.Reason,
            Quantity = request.Quantity,
            Priority = request.Priority ?? "Normal",
            RequestType = request.RequestType ?? "New Asset",
            Status = Domain.Enums.RequestStatus.Pending,
            UserId = userId,
            DivisionId = divisionId,
            AssetId = request.AssetId,
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