using FluentValidation;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Platform.Tenants.ManageTenant;

public sealed record GetPlatformTenantByIdQuery(Guid TenantId) : IQuery<PlatformTenantDto>;
public sealed record GetPlatformTenantBySlugQuery(string Slug) : IQuery<PlatformTenantDto>;
public sealed record UpdatePlatformTenantCommand(Guid TenantId, string Name) : ICommand<PlatformTenantDto>;
public sealed record ChangePlatformTenantStatusCommand(Guid TenantId, bool IsActive) : ICommand<PlatformTenantDto>;

public sealed class UpdatePlatformTenantCommandValidator : AbstractValidator<UpdatePlatformTenantCommand>
{
    public UpdatePlatformTenantCommandValidator() =>
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
}

public sealed class GetPlatformTenantBySlugQueryHandler(IPlatformTenantRepository tenants)
    : IQueryHandler<GetPlatformTenantBySlugQuery, PlatformTenantDto>
{
    public async Task<Result<PlatformTenantDto>> Handle(GetPlatformTenantBySlugQuery query, CancellationToken ct)
    {
        var tenant = await tenants.GetBySlugAsync(query.Slug, ct);
        return tenant is null
            ? Result.Failure<PlatformTenantDto>(Error.NotFound("Tenant not found."))
            : Result.Success(tenant.ToPlatformDto());
    }
}

public sealed class GetPlatformTenantByIdQueryHandler(IPlatformTenantRepository tenants)
    : IQueryHandler<GetPlatformTenantByIdQuery, PlatformTenantDto>
{
    public async Task<Result<PlatformTenantDto>> Handle(GetPlatformTenantByIdQuery query, CancellationToken ct)
    {
        var tenant = await tenants.GetAsync(query.TenantId, ct);
        return tenant is null
            ? Result.Failure<PlatformTenantDto>(Error.NotFound("Tenant not found."))
            : Result.Success(tenant.ToPlatformDto());
    }
}

public sealed class UpdatePlatformTenantCommandHandler(
    IPlatformTenantRepository tenants,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdatePlatformTenantCommand, PlatformTenantDto>
{
    public async Task<Result<PlatformTenantDto>> Handle(UpdatePlatformTenantCommand command, CancellationToken ct)
    {
        var tenant = await tenants.GetAsync(command.TenantId, ct);
        if (tenant is null) return Result.Failure<PlatformTenantDto>(Error.NotFound("Tenant not found."));

        tenant.Rename(command.Name);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(tenant.ToPlatformDto());
    }
}

public sealed class ChangePlatformTenantStatusCommandHandler(
    IPlatformTenantRepository tenants,
    IAuthSessionRepository sessions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<ChangePlatformTenantStatusCommand, PlatformTenantDto>
{
    public async Task<Result<PlatformTenantDto>> Handle(ChangePlatformTenantStatusCommand command, CancellationToken ct)
    {
        var tenant = await tenants.GetAsync(command.TenantId, ct);
        if (tenant is null) return Result.Failure<PlatformTenantDto>(Error.NotFound("Tenant not found."));

        if (command.IsActive) tenant.Activate();
        else
        {
            tenant.Deactivate();
            var now = timeProvider.GetUtcNow();
            foreach (var session in await sessions.ListActiveByTenantAsync(tenant.Id, ct)) session.Revoke(now);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(tenant.ToPlatformDto());
    }
}
