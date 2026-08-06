using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Authentication;
using Ordivo.Domain.Users;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Authentication.Identity;

public sealed record VerifyEmailCommand(string Token) : ICommand<bool>;
public sealed record ResendVerificationCommand(string Email) : ICommand<bool>;
public sealed record ForgotPasswordCommand(string Email) : ICommand<bool>;
public sealed record ResetPasswordCommand(string Token, string NewPassword) : ICommand<bool>;
public sealed record AcceptInvitationCommand(string Token, string Password) : ICommand<bool>;

public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator() => RuleFor(command => command.Token).NotEmpty();
}
public sealed class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationCommandValidator() => RuleFor(command => command.Email).NotEmpty().EmailAddress();
}
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator() => RuleFor(command => command.Email).NotEmpty().EmailAddress();
}
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty();
        RuleFor(command => command.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty();
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public sealed class VerifyEmailCommandHandler(
    IIdentityTokenRepository tokens,
    IIdentityTokenGenerator tokenGenerator,
    IUserRepository users,
    IPlatformTenantRepository tenants,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<VerifyEmailCommand, bool>
{
    public async Task<Result<bool>> Handle(VerifyEmailCommand command, CancellationToken ct)
    {
        var token = await tokens.GetByHashAsync(tokenGenerator.Hash(command.Token), IdentityTokenType.EmailVerification, ct);
        var now = timeProvider.GetUtcNow();
        if (token is null || !token.IsValid(now))
            return Result.Failure<bool>(Error.Validation("The verification token is invalid or expired."));
        var user = await users.GetByIdAsync(token.UserId, ct);
        if (user is null || user.TenantId != token.TenantId)
            return Result.Failure<bool>(Error.Validation("The verification token is invalid or expired."));
        var tenant = await tenants.GetAsync(user.TenantId, ct);
        if (tenant is null || !tenant.IsActive) return Result.Failure<bool>(Error.Forbidden("Tenant is suspended."));
        user.VerifyEmail(now);
        token.Consume(now);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}

public sealed class ResendVerificationCommandHandler(
    IUserRepository users,
    IIdentityTokenRepository tokens,
    IIdentityTokenGenerator tokenGenerator,
    IIdentityEmailSender emailSender,
    IPlatformTenantRepository tenants,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<ResendVerificationCommand, bool>
{
    public async Task<Result<bool>> Handle(ResendVerificationCommand command, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(User.NormalizeEmail(command.Email), ct);
        if (user is null || !user.IsActive || user.IsEmailVerified) return Result.Success(true);
        var tenant = await tenants.GetAsync(user.TenantId, ct);
        if (tenant is null || !tenant.IsActive) return Result.Success(true);
        var now = timeProvider.GetUtcNow();
        await tokens.ConsumeActiveAsync(user.Id, IdentityTokenType.EmailVerification, now, ct);
        var generated = tokenGenerator.Generate(TimeSpan.FromHours(24));
        await tokens.AddAsync(IdentityToken.Create(user.Id, user.TenantId, user.Email,
            IdentityTokenType.EmailVerification, generated.Hash, generated.ExpiresAt), ct);
        await unitOfWork.SaveChangesAsync(ct);
        await emailSender.SendEmailVerificationAsync(user.Email, user.Name, generated.Token, ct);
        return Result.Success(true);
    }
}

public sealed class ForgotPasswordCommandHandler(
    IUserRepository users,
    IIdentityTokenRepository tokens,
    IIdentityTokenGenerator tokenGenerator,
    IIdentityEmailSender emailSender,
    IPlatformTenantRepository tenants,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<ForgotPasswordCommand, bool>
{
    public async Task<Result<bool>> Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(User.NormalizeEmail(command.Email), ct);
        if (user is null || !user.IsActive) return Result.Success(true);
        var tenant = await tenants.GetAsync(user.TenantId, ct);
        if (tenant is null || !tenant.IsActive) return Result.Success(true);
        var now = timeProvider.GetUtcNow();
        await tokens.ConsumeActiveAsync(user.Id, IdentityTokenType.PasswordReset, now, ct);
        var generated = tokenGenerator.Generate(TimeSpan.FromHours(1));
        await tokens.AddAsync(IdentityToken.Create(user.Id, user.TenantId, user.Email,
            IdentityTokenType.PasswordReset, generated.Hash, generated.ExpiresAt), ct);
        await unitOfWork.SaveChangesAsync(ct);
        await emailSender.SendPasswordResetAsync(user.Email, user.Name, generated.Token, ct);
        return Result.Success(true);
    }
}

public sealed class ResetPasswordCommandHandler(
    IIdentityTokenRepository tokens,
    IIdentityTokenGenerator tokenGenerator,
    IUserRepository users,
    IPlatformTenantRepository tenants,
    IPasswordHasher passwordHasher,
    IAuthSessionRepository sessions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<ResetPasswordCommand, bool>
{
    public async Task<Result<bool>> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var token = await tokens.GetByHashAsync(tokenGenerator.Hash(command.Token), IdentityTokenType.PasswordReset, ct);
        var now = timeProvider.GetUtcNow();
        if (token is null || !token.IsValid(now))
            return Result.Failure<bool>(Error.Validation("The password reset token is invalid or expired."));
        var user = await users.GetByIdAsync(token.UserId, ct);
        if (user is null || user.TenantId != token.TenantId || !user.IsActive)
            return Result.Failure<bool>(Error.Validation("The password reset token is invalid or expired."));
        var tenant = await tenants.GetAsync(user.TenantId, ct);
        if (tenant is null || !tenant.IsActive) return Result.Failure<bool>(Error.Forbidden("Tenant is suspended."));
        user.ChangePassword(passwordHasher.Hash(command.NewPassword));
        user.VerifyEmail(now);
        token.Consume(now);
        foreach (var session in await sessions.ListActiveByUserAsync(user.Id, AuthSubjectType.TenantUser, ct))
            session.Revoke(now);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}

public sealed class AcceptInvitationCommandHandler(
    IIdentityTokenRepository tokens,
    IIdentityTokenGenerator tokenGenerator,
    IUserRepository users,
    IPlatformTenantRepository tenants,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<AcceptInvitationCommand, bool>
{
    public async Task<Result<bool>> Handle(AcceptInvitationCommand command, CancellationToken ct)
    {
        var token = await tokens.GetByHashAsync(tokenGenerator.Hash(command.Token), IdentityTokenType.UserInvitation, ct);
        var now = timeProvider.GetUtcNow();
        if (token is null || !token.IsValid(now))
            return Result.Failure<bool>(Error.Validation("The invitation is invalid or expired."));
        var user = await users.GetByIdAsync(token.UserId, ct);
        if (user is null || user.TenantId != token.TenantId)
            return Result.Failure<bool>(Error.Validation("The invitation is invalid or expired."));
        var tenant = await tenants.GetAsync(user.TenantId, ct);
        if (tenant is null || !tenant.IsActive) return Result.Failure<bool>(Error.Forbidden("Tenant is suspended."));
        user.ChangePassword(passwordHasher.Hash(command.Password));
        user.VerifyEmail(now);
        user.Activate();
        token.Consume(now);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
