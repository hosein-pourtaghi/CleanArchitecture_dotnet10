using FluentValidation;

namespace Application.Customers.Update;

/// <summary>
/// Validator for UpdateCustomerCommand.
/// Ensures all required fields are valid before processing.
/// </summary>
internal sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Customer ID is required.");

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
