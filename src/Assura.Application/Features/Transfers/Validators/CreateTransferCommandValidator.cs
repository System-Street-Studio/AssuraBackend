using FluentValidation;
using Assura.Application.Features.Transfers.Commands;

namespace Assura.Application.Features.Transfers.Validators;

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.AssetId)
            .GreaterThan(0).WithMessage("Asset ID is required.");

        RuleFor(x => x.AssetRequestId)
            .GreaterThan(0).When(x => x.AssetRequestId.HasValue)
            .WithMessage("Asset Request ID must be greater than 0 if provided.");
    }
}
