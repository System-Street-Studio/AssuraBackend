using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.LostItems.Commands.Create;

public class CreateLostItemCommand : IRequest<int>
{
    public string AssetName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? AssetId { get; set; }
}

public class CreateLostItemCommandValidator : AbstractValidator<CreateLostItemCommand>
{
    public CreateLostItemCommandValidator()
    {
        RuleFor(x => x.AssetName)
            .NotEmpty().WithMessage("Asset name is required.")
            .MaximumLength(200).WithMessage("Asset name cannot exceed 200 characters.");

        RuleFor(x => x.Division)
            .NotEmpty().WithMessage("Division is required.")
            .MaximumLength(100).WithMessage("Division cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
    }
}

public class CreateLostItemCommandHandler : IRequestHandler<CreateLostItemCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateLostItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(CreateLostItemCommand request, CancellationToken cancellationToken)
    {
        decimal purchasePrice = 0;
        decimal currentValue = 0;

        int? resolvedAssetId = request.AssetId;
        Asset? matchedAsset = null;

        if (resolvedAssetId.HasValue)
        {
            matchedAsset = await _context.Assets.FindAsync(new object[] { resolvedAssetId.Value }, cancellationToken);
        }
        else
        {
            matchedAsset = await _context.Assets.FirstOrDefaultAsync(a => 
                a.AssetTag == request.AssetName || 
                a.AssetCode == request.AssetName || 
                (a.Product != null && a.Product.Name == request.AssetName), cancellationToken);
            if (matchedAsset != null)
            {
                resolvedAssetId = matchedAsset.Id;
            }
        }

        if (matchedAsset != null)
        {
            purchasePrice = matchedAsset.PurchaseValue;
            currentValue = matchedAsset.PurchaseValue;
            matchedAsset.Status = AssetStatus.Lost;
            matchedAsset.AssignedUserId = null;
        }

        var reportedByName = await ResolveActingUserNameAsync(cancellationToken);
        var now = DateTime.UtcNow;

        var lostItem = new LostItem
        {
            AssetName = request.AssetName,
            Division = request.Division,
            Date = now,
            Time = now.TimeOfDay,
            ReportedBy = reportedByName,
            Status = LostItemStatus.Reported,
            AssetType = request.AssetType,
            ValueAtPurchasing = purchasePrice,
            CurrentValue = currentValue,
            Description = request.Description,
            AssetId = resolvedAssetId
        };

        _context.LostItems.Add(lostItem);

        var superintendents = await _context.Users
            .Where(u => u.Role == UserRole.Superintendent || u.Role == UserRole.Admin)
            .ToListAsync(cancellationToken);

        foreach (var s in superintendents)
        {
            _context.Notifications.Add(new Notification
            {
                Title = "Asset Reported Lost",
                Message = $"Asset '{request.AssetName}' from {request.Division} was reported lost by {reportedByName}.",
                UserId = s.Id,
                Type = "Info",
                ReferenceId = lostItem.Id.ToString()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return lostItem.Id;
    }

    private async Task<string> ResolveActingUserNameAsync(CancellationToken cancellationToken)
    {
        if (int.TryParse(_currentUserService.UserId, out var actingUserId))
        {
            var actingUser = await _context.Users.FindAsync(new object[] { actingUserId }, cancellationToken);
            if (actingUser != null)
            {
                var fullName = $"{actingUser.FirstName} {actingUser.LastName}".Trim();
                if (!string.IsNullOrEmpty(fullName))
                {
                    return fullName;
                }
            }
        }

        return "Unknown";
    }
}
