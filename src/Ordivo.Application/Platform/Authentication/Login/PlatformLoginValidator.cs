using FluentValidation;

namespace Ordivo.Application.Platform.Authentication.Login;

public sealed class PlatformLoginCommandValidator : AbstractValidator<PlatformLoginCommand>
{
    public PlatformLoginCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(command => command.Password).NotEmpty().MaximumLength(128);
    }
}
