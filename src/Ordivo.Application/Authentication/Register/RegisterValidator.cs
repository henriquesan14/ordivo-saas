using FluentValidation;

namespace Ordivo.Application.Authentication.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.PlanId).NotEmpty();
        RuleFor(command => command.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(120);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
