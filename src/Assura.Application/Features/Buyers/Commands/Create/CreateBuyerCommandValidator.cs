using FluentValidation;

namespace Assura.Application.Features.Buyers.Commands.Create;

public class CreateBuyerCommandValidator : AbstractValidator<CreateBuyerCommand>
{
    public CreateBuyerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Buyer/Company name is required.")
            .MinimumLength(2).WithMessage("Buyer/Company name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Buyer/Company name cannot exceed 100 characters.");

        RuleFor(x => x.Contact)
            .NotEmpty().WithMessage("Contact person is required.")
            .MinimumLength(2).WithMessage("Contact person must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Contact person cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^0\d{9}$").WithMessage("Phone number must start with 0 and be exactly 10 digits.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Specialization / Category is required.")
            .MinimumLength(2).WithMessage("Category must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters.");
    }
}
