using FluentValidation;

namespace Ordivo.Application.ServiceOrders.CreateServiceOrder;

public sealed class CreateServiceOrderCommandValidator : AbstractValidator<CreateServiceOrderCommand>
{
    public CreateServiceOrderCommandValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MaximumLength(160);
        RuleFor(command => command.Description).MaximumLength(4000);
        RuleFor(command => command.Price).GreaterThanOrEqualTo(0);
    }
}
