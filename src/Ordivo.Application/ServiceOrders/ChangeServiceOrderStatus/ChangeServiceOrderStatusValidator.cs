using FluentValidation;

namespace Ordivo.Application.ServiceOrders.ChangeServiceOrderStatus;

public sealed class ChangeServiceOrderStatusCommandValidator : AbstractValidator<ChangeServiceOrderStatusCommand>
{
    public ChangeServiceOrderStatusCommandValidator()
    {
        RuleFor(command => command.ServiceOrderId).NotEmpty();
        RuleFor(command => command.Status).IsInEnum();
    }
}
