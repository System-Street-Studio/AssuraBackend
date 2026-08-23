using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Domain.Enums;
using FluentValidation;

namespace Assura.Application.Features.LostItems.Commands.UpdateStatus;

public class UpdateLostItemStatusCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateLostItemStatusCommandValidator : AbstractValidator<UpdateLostItemStatusCommand>
{
    public UpdateLostItemStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Lost Item ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => Enum.TryParse<LostItemStatus>(status, true, out _))
            .WithMessage(x => $"'{x.Status}' is not a valid lost item status.");
    }
}

public class UpdateLostItemStatusCommandHandler : IRequestHandler<UpdateLostItemStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateLostItemStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateLostItemStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.LostItems.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return false;

        if (!Enum.TryParse<LostItemStatus>(request.Status, true, out var status))
        {
            return false;
        }

        entity.Status = status;

        if (status == LostItemStatus.ConfirmedLost && entity.AssetId.HasValue)
        {
            var asset = await _context.Assets.FindAsync(new object[] { entity.AssetId.Value }, cancellationToken);
            if (asset != null)
            {
                asset.Status = AssetStatus.Lost;
                asset.AssignedUserId = null;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
