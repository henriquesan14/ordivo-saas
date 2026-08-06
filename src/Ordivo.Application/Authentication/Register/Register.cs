using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Users;
using Ordivo.Domain.Tenants;
using Ordivo.Domain.Authentication;
using Ordivo.Domain.Commercial;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Authentication.Register;

public sealed record RegisterCommand(string TenantName, string Name, string Email, string Password) : ICommand<RegistrationDto>;
public sealed record RegistrationDto(Guid UserId, Guid TenantId, string Email, bool EmailVerificationRequired);

public sealed class RegisterCommandHandler(
    IUserRepository users,
    ITenantRepository tenants,
    IPasswordHasher passwordHasher,
    IIdentityTokenGenerator tokenGenerator,
    IIdentityTokenRepository identityTokens,
    IIdentityEmailSender emailSender,
    ICommercialRepository commercial,
    TimeProvider clock,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterCommand, RegistrationDto>
{
    public async Task<Result<RegistrationDto>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var email = User.NormalizeEmail(command.Email);
        if (await users.EmailExistsAsync(email, ct))
            return Result.Failure<RegistrationDto>(Error.Conflict("A user with this email already exists."));

        var tenant = Tenant.Create(command.TenantName);
        var user = User.Create(tenant.Id, command.Name, email, passwordHasher.Hash(command.Password));
        var verificationToken = tokenGenerator.Generate(TimeSpan.FromHours(24));
        await tenants.AddAsync(tenant, ct);
        await users.AddAsync(user, ct);
        var defaultPlan = (await commercial.ListPlansAsync(true, ct)).FirstOrDefault();
        if (defaultPlan is not null) await commercial.AddSubscriptionAsync(Subscription.Start(tenant.Id, defaultPlan, clock.GetUtcNow()), ct);
        await identityTokens.AddAsync(IdentityToken.Create(
            user.Id, tenant.Id, user.Email, IdentityTokenType.EmailVerification,
            verificationToken.Hash, verificationToken.ExpiresAt), ct);
        await emailSender.SendEmailVerificationAsync(user.Email, user.Name, verificationToken.Token, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(new RegistrationDto(user.Id, tenant.Id, user.Email, true));
    }
}
