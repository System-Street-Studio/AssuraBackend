using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Assets.Queries;
using Assura.Domain.Constants;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Assura.Application.Features.Assets.Commands;

/// <summary>
/// Command to process the checkout of an asset to an employee.
/// Contains the asset ID, assignee ID, due date, and checkout details.
/// </summary>
public record CheckoutAssetCommand(int AssetId, int AssigneeUserId, DateOnly DueDate, string? Notes, string? CheckedOutBy) : IRequest<CheckoutRecordDto>;

internal class CheckoutRecordMeta
{
    public DateOnly DueDate { get; set; }
    public string? CheckedOutBy { get; set; }
}

public class CheckoutAssetCommandValidator : AbstractValidator<CheckoutAssetCommand>
{
    public CheckoutAssetCommandValidator()
    {
        RuleFor(x => x.AssetId)
            .GreaterThan(0);

        RuleFor(x => x.AssigneeUserId)
            .GreaterThan(0);

        RuleFor(x => x.DueDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Due date cannot be in the past.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}

/// <summary>
/// Handler for executing the <see cref="CheckoutAssetCommand"/>.
/// Validates availability, updates asset status, and creates a checkout request record.
/// </summary>
public class CheckoutAssetCommandHandler : IRequestHandler<CheckoutAssetCommand, CheckoutRecordDto>
{
    private readonly IApplicationDbContext _context;

    public CheckoutAssetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CheckoutRecordDto> Handle(CheckoutAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _context.Assets
            .Include(a => a.Product)
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);

        if (asset == null)
        {
            throw new ValidationException("Asset not found.");
        }

        var assignee = await _context.Users
            .Include(u => u.Division)
            .FirstOrDefaultAsync(u => u.Id == request.AssigneeUserId && u.IsActive, cancellationToken);

        if (assignee == null)
        {
            throw new ValidationException("Assignee user not found or inactive.");
        }

        if ((asset.Status != AssetStatus.InStore && (int)asset.Status != 0) && !(asset.Status == AssetStatus.InUse && asset.AssignedUserId == assignee.Id))
        {
            throw new ValidationException("Asset is no longer available for checkout.");
        }

        var activeCheckout = await _context.Requests
            .AnyAsync(r => r.AssetId == asset.Id && r.Status == RequestWorkflowStatus.CheckedOut, cancellationToken);
        if (activeCheckout)
        {
            throw new ValidationException($"Asset {asset.AssetCode} is already checked out.");
        }

        if (asset.AssignedUserId.HasValue && asset.AssignedUserId.Value != assignee.Id)
        {
            throw new ValidationException($"Asset {asset.AssetCode} is already assigned to another user.");
        }

        var requestNumber = $"CHK-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var checkoutMeta = new CheckoutRecordMeta
        {
            DueDate = request.DueDate,
            CheckedOutBy = string.IsNullOrWhiteSpace(request.CheckedOutBy) ? "Storekeeper" : request.CheckedOutBy.Trim()
        };

        var checkoutRequest = new Request
        {
            RequestNumber = requestNumber,
            Type = RequestType.Asset,
            Priority = PriorityType.Medium,
            Description = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            RequesterId = assignee.Id,
            AssetId = asset.Id,
            Status = RequestWorkflowStatus.CheckedOut,
            Remarks = JsonSerializer.Serialize(checkoutMeta)
        };

        asset.AssignedUserId = assignee.Id;
        asset.Status = AssetStatus.InUse;

        _context.Requests.Add(checkoutRequest);

        // Auto-generate a GIN (Goods Issue Note) upon checkout
        var grn = await _context.GRNs
            .FirstOrDefaultAsync(g => g.AssetId == asset.Id, cancellationToken);
        if (grn == null && asset.PurchasingOrderId.HasValue)
        {
            grn = await _context.GRNs
                .FirstOrDefaultAsync(g => g.PurchasingOrderId == asset.PurchasingOrderId.Value, cancellationToken);
        }
        if (grn == null)
        {
            grn = await _context.GRNs
                .OrderByDescending(g => g.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (grn != null)
        {
            var ginNumber = $"GIN-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            var gin = new GIN
            {
                GinNumber = ginNumber,
                AssignedDate = DateTime.UtcNow,
                Condition = "Good",
                Notes = string.IsNullOrWhiteSpace(request.Notes)
                    ? $"Issued upon checkout to {assignee.FirstName} {assignee.LastName} (Ref: {checkoutRequest.RequestNumber})"
                    : request.Notes.Trim(),
                GRNId = grn.Id,
                AssetId = asset.Id
            };
            _context.GINs.Add(gin);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CheckoutRecordDto
        {
            Id = checkoutRequest.RequestNumber,
            AssetId = asset.Id.ToString(),
            AssetName = asset.Product?.Name ?? asset.AssetCode,
            Category = asset.Category?.Name ?? "-",
            Serial = string.IsNullOrWhiteSpace(asset.SerialNumber) ? "-" : asset.SerialNumber,
            CheckedOutTo = (assignee.FirstName + " " + assignee.LastName).Trim(),
            Division = assignee.Division?.Name ?? "N/A",
            Email = assignee.Email,
            CheckoutDate = DateOnly.FromDateTime(checkoutRequest.CreatedAt).ToString("yyyy-MM-dd"),
            DueDate = request.DueDate.ToString("yyyy-MM-dd"),
            Status = RequestWorkflowStatus.CheckedOut,
            CheckoutNotes = checkoutRequest.Description,
            CheckedOutBy = checkoutMeta.CheckedOutBy ?? "Storekeeper"
        };
    }
}
