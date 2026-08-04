using FluentValidation;

namespace Ordivo.Application.Tenants.UpdateTenant;

public sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator() =>
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
}
