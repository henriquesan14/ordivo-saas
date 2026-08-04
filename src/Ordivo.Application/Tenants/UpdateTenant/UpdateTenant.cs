using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Tenants.UpdateTenant;

public sealed record UpdateTenantCommand(string Name) : ICommand<TenantDto>;

public sealed class UpdateTenantCommandHandler(
    ITenantRepository tenants,
    IUserContext userContext,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateTenantCommand, TenantDto>
{
    public async Task<Result<TenantDto>> Handle(UpdateTenantCommand command, CancellationToken ct)
    {
        var tenant = await tenants.GetAsync(userContext.TenantId, ct);
        if (tenant is null) return Result.Failure<TenantDto>(Error.NotFound("Tenant not found."));

        tenant.Rename(command.Name);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(tenant.ToDto());
    }
}
