using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Impersonation;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Platform.Impersonation;

public sealed record StartImpersonationCommand(Guid TenantId, Guid? UserId, string Reason) : ICommand<ImpersonationDto>;
public sealed record EndImpersonationCommand : ICommand<EndImpersonationDto>;
public sealed record EndImpersonationDto(string AccessToken, DateTimeOffset ExpiresAt);
public sealed record ImpersonationDto(Guid SessionId, Guid TenantId, Guid UserId, string UserName, string UserEmail, string Role, string Reason, DateTimeOffset ExpiresAt, string AccessToken);
public sealed class StartImpersonationValidator : AbstractValidator<StartImpersonationCommand>
{
    public StartImpersonationValidator(){RuleFor(x=>x.TenantId).NotEmpty();RuleFor(x=>x.Reason).NotEmpty().MinimumLength(10).MaximumLength(500);}
}
public sealed class StartImpersonationHandler(IPlatformTenantRepository tenants,IPlatformUserRepository platformUsers,IImpersonationRepository impersonations,IGenerateToken tokens,IUserContext context,IUnitOfWork uow,TimeProvider clock):ICommandHandler<StartImpersonationCommand,ImpersonationDto>
{
    public async Task<Result<ImpersonationDto>> Handle(StartImpersonationCommand c,CancellationToken ct)
    {
        var tenant=await tenants.GetAsync(c.TenantId,ct); if(tenant is null||!tenant.IsActive)return Result.Failure<ImpersonationDto>(Error.Forbidden("Tenant not found or suspended."));
        var platformUser=await platformUsers.GetByIdAsync(context.UserId,ct); if(platformUser is null||!platformUser.IsActive)return Result.Failure<ImpersonationDto>(Error.Forbidden("Platform administrator is not active."));
        var target=await impersonations.GetTargetUserAsync(c.TenantId,c.UserId,ct); if(target is null)return Result.Failure<ImpersonationDto>(Error.NotFound("No active verified user was found for this tenant."));
        var now=clock.GetUtcNow();var session=ImpersonationSession.Start(platformUser.Id,tenant.Id,target.Id,c.Reason,now,TimeSpan.FromMinutes(15));await impersonations.AddAsync(session,ct);
        var token=tokens.GenerateImpersonationToken(target,platformUser.Id,platformUser.Name,session.Id,session.ExpiresAt);await uow.SaveChangesAsync(ct);
        return Result.Success(new ImpersonationDto(session.Id,tenant.Id,target.Id,target.Name,target.Email,target.Role.ToString(),session.Reason,session.ExpiresAt,token.Token));
    }
}
public sealed class EndImpersonationHandler(IImpersonationRepository impersonations,IPlatformUserRepository platformUsers,IGenerateToken tokens,IUserContext context,IUnitOfWork uow,TimeProvider clock):ICommandHandler<EndImpersonationCommand,EndImpersonationDto>
{
    public async Task<Result<EndImpersonationDto>> Handle(EndImpersonationCommand c,CancellationToken ct)
    {
        if(!context.IsImpersonating||context.ImpersonationSessionId is not Guid id||context.ImpersonatorUserId is not Guid impersonatorId)return Result.Failure<EndImpersonationDto>(Error.Forbidden("No active impersonation session."));
        var session=await impersonations.GetSessionAsync(id,ct);if(session is null||session.PlatformUserId!=impersonatorId)return Result.Failure<EndImpersonationDto>(Error.NotFound("Impersonation session not found."));
        var platformUser=await platformUsers.GetByIdAsync(impersonatorId,ct);if(platformUser is null||!platformUser.IsActive)return Result.Failure<EndImpersonationDto>(Error.Forbidden("Platform administrator is not active."));
        session.End(clock.GetUtcNow());var token=tokens.GenerateToken(platformUser);await uow.SaveChangesAsync(ct);return Result.Success(new EndImpersonationDto(token.Token,token.ExpiresAt));
    }
}
