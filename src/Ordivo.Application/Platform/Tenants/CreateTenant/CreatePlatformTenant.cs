using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Tenants;
using Ordivo.Domain.Users;
using Ordivo.Domain.Commercial;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Platform.Tenants.CreateTenant;

public sealed record CreatePlatformTenantCommand(
    string TenantName,
    string OwnerName,
    string OwnerEmail,
    string OwnerPassword) : ICommand<CreatePlatformTenantDto>;

public sealed record CreatePlatformTenantDto(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    bool IsActive,
    Guid OwnerUserId,
    string OwnerName,
    string OwnerEmail,
    UserRole OwnerRole,
    DateTimeOffset CreatedAt);

public sealed class CreatePlatformTenantCommandHandler(
    ITenantRepository tenants,
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ICommercialRepository commercial,
    TimeProvider clock,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePlatformTenantCommand, CreatePlatformTenantDto>
{
    public async Task<Result<CreatePlatformTenantDto>> Handle(
        CreatePlatformTenantCommand command,
        CancellationToken ct)
    {
        var normalizedEmail = User.NormalizeEmail(command.OwnerEmail);
        if (await users.EmailExistsAsync(normalizedEmail, ct))
            return Result.Failure<CreatePlatformTenantDto>(Error.Conflict("A user with this email already exists."));

        var tenant = Tenant.Create(command.TenantName);
        var owner = User.Create(
            tenant.Id,
            command.OwnerName,
            normalizedEmail,
            passwordHasher.Hash(command.OwnerPassword),
            UserRole.Owner);
        owner.VerifyEmail(clock.GetUtcNow());

        await tenants.AddAsync(tenant, ct);
        await users.AddAsync(owner, ct);
        var defaultPlan = (await commercial.ListPlansAsync(true, ct)).FirstOrDefault();
        if (defaultPlan is not null) await commercial.AddSubscriptionAsync(Subscription.Start(tenant.Id, defaultPlan, clock.GetUtcNow()), ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new CreatePlatformTenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.IsActive,
            owner.Id,
            owner.Name,
            owner.Email,
            owner.Role,
            tenant.CreatedAt));
    }
}
