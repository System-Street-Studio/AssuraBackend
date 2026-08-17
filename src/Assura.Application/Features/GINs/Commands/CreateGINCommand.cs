using Assura.Application.Common.Interfaces;
using Assura.Application.Features.GINs.Queries;
using Assura.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Features.GINs.Commands;

/// <summary>
/// Command to record a Goods Issue Note for an asset being handed out of the store
/// (e.g. on checkout) — the Storekeeper's record of "goods physically left the
/// store", the counterpart to a GRN's "goods physically arrived". Referencing the
/// GRN the asset arrived under ties the outgoing paperwork trail back to the
/// incoming one.
/// </summary>
public record CreateGINCommand(int GRNId, int AssetId, DateTime AssignedDate, string? Condition, string? Notes) : IRequest<GINDto>;

public class CreateGINCommandValidator : AbstractValidator<CreateGINCommand>
{
    public CreateGINCommandValidator()
    {
        RuleFor(x => x.GRNId).GreaterThan(0);
        RuleFor(x => x.AssetId).GreaterThan(0);
        RuleFor(x => x.AssignedDate)
            .Must(d => d <= DateTime.UtcNow.AddDays(1))
            .WithMessage("Assigned date cannot be in the future.");
        RuleFor(x => x.Condition).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class CreateGINCommandHandler : IRequestHandler<CreateGINCommand, GINDto>
{
    private readonly IApplicationDbContext _context;

    public CreateGINCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GINDto> Handle(CreateGINCommand request, CancellationToken cancellationToken)
    {
        var grn = await _context.GRNs
            .FirstOrDefaultAsync(g => g.Id == request.GRNId, cancellationToken);
        if (grn == null)
        {
            throw new ValidationException("GRN not found.");
        }

        var asset = await _context.Assets
            .Include(a => a.Product)
            .Include(a => a.AssignedUser)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);
        if (asset == null)
        {
            throw new ValidationException("Asset not found.");
        }

        // A GIN issues out the exact asset the referenced GRN brought in — the two
        // records are meant to describe the same physical item's full paper trail.
        if (grn.AssetId != request.AssetId)
        {
            throw new ValidationException("This asset does not match the asset received under the specified GRN.");
        }

        var alreadyIssued = await _context.GINs
            .AnyAsync(g => g.AssetId == request.AssetId, cancellationToken);
        if (alreadyIssued)
        {
            throw new ValidationException("A GIN has already been recorded for this asset.");
        }

        var ginNumber = $"GIN-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var gin = new GIN
        {
            GinNumber = ginNumber,
            AssignedDate = request.AssignedDate,
            Condition = request.Condition,
            Notes = request.Notes,
            GRNId = request.GRNId,
            AssetId = request.AssetId,
        };

        _context.GINs.Add(gin);
        await _context.SaveChangesAsync(cancellationToken);

        return new GINDto
        {
            Id = gin.Id,
            GinNumber = gin.GinNumber,
            AssignedDate = gin.AssignedDate,
            Condition = gin.Condition,
            Notes = gin.Notes,
            GRNId = grn.Id,
            GrnNumber = grn.GrnNumber,
            AssetId = asset.Id,
            AssetCode = asset.AssetCode,
            ProductName = asset.Product?.Name ?? "-",
            AssignedUserName = asset.AssignedUser != null
                ? $"{asset.AssignedUser.FirstName} {asset.AssignedUser.LastName}"
                : null,
            CreatedAt = gin.CreatedAt,
        };
    }
}
