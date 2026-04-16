using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Assets.Queries;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Assura.Application.Features.Assets.Commands;

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

        if (asset.Status != AssetStatus.InStore || asset.AssignedUserId != null)
        {
            throw new ValidationException("Asset is no longer available for checkout.");
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
            Status = "Checked Out",
            Remarks = JsonSerializer.Serialize(checkoutMeta)
        };

        asset.AssignedUserId = assignee.Id;
        asset.Status = AssetStatus.InUse;

        _context.Requests.Add(checkoutRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return new CheckoutRecordDto
        {
            Id = checkoutRequest.RequestNumber,
            AssetId = asset.Id.ToString(),
            AssetName = asset.Product?.Name ?? asset.AssetCode,
            Category = asset.Category?.Name ?? "-",
            Serial = string.IsNullOrWhiteSpace(asset.SerialNumber) ? "-" : asset.SerialNumber,
            CheckedOutTo = (assignee.FirstName + " " + assignee.LastName).Trim(),
            Department = assignee.Division?.Name ?? "N/A",
            Email = assignee.Email,
            CheckoutDate = DateOnly.FromDateTime(checkoutRequest.CreatedAt).ToString("yyyy-MM-dd"),
            DueDate = request.DueDate.ToString("yyyy-MM-dd"),
            Status = "Checked Out",
            CheckoutNotes = checkoutRequest.Description,
            CheckedOutBy = checkoutMeta.CheckedOutBy ?? "Storekeeper"
        };
    }
}
