using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Ordivo.Application.Abstractions.Authentication;

namespace Ordivo.Infrastructure.Authentication;

internal sealed class RefreshTokenGenerator(
    IOptions<RefreshTokenOptions> options,
    TimeProvider timeProvider) : IRefreshTokenGenerator
{
    public RefreshToken Generate()
    {
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        var expiresAt = timeProvider.GetUtcNow().AddDays(options.Value.ExpirationDays);
        return new RefreshToken(token, Hash(token), expiresAt);
    }

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
