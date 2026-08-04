using FluentValidation;

namespace Ordivo.Application.Platform.Tenants.CreateTenant;

public sealed class CreatePlatformTenantCommandValidator : AbstractValidator<CreatePlatformTenantCommand>
{
    public CreatePlatformTenantCommandValidator()
    {
        RuleFor(command => command.TenantName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.OwnerName).NotEmpty().MaximumLength(120);
        RuleFor(command => command.OwnerEmail).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(command => command.OwnerPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
