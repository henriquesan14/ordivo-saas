using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Users;
using Ordivo.Domain.Tenants;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Authentication.Register;

public sealed record RegisterCommand(string TenantName, string Name, string Email, string Password) : ICommand<AuthDto>;

public sealed class RegisterCommandHandler(
    IUserRepository users,
    ITenantRepository tenants,
    IPasswordHasher passwordHasher,
    IGenerateToken tokenGenerator,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterCommand, AuthDto>
{
    public async Task<Result<AuthDto>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var email = User.NormalizeEmail(command.Email);
        if (await users.EmailExistsAsync(email, ct))
            return Result.Failure<AuthDto>(Error.Conflict("A user with this email already exists."));

        var tenant = Tenant.Create(command.TenantName);
        var user = User.Create(tenant.Id, command.Name, email, passwordHasher.Hash(command.Password));
        await tenants.AddAsync(tenant, ct);
        await users.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(user.ToAuthDto(tokenGenerator.GenerateToken(user)));
    }
}
