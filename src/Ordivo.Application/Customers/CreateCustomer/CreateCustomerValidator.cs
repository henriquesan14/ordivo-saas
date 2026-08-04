using FluentValidation;

namespace Ordivo.Application.Customers.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(120);
        RuleFor(command => command.Document).NotEmpty().MaximumLength(20);
        RuleFor(command => command.Phone).NotEmpty().MaximumLength(20);
        RuleFor(command => command.Email).EmailAddress().MaximumLength(254)
            .When(command => !string.IsNullOrWhiteSpace(command.Email));
    }
}
