using FluentValidation;

namespace Application.Customers.Create;

/// <summary>
/// Validator for CreateCustomerCommand.
/// Ensures all required fields are valid before processing.
/// </summary>
internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Customer name is required.")
            .MaximumLength(200)
            .WithMessage("Customer name cannot exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email address is required.")
            .EmailAddress()
            .WithMessage("Email address must be valid.")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone number cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));
    }
}
