using Microsoft.AspNetCore.Identity;
using Ordivo.Application.Abstractions.Authentication;

namespace Ordivo.Infrastructure.Authentication;

internal sealed class PasswordHasher : IPasswordHasher
{
    private static readonly object UserMarker = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(UserMarker, password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(UserMarker, passwordHash, password) is not PasswordVerificationResult.Failed;
}
