using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Ordivo.Application.Abstractions.Authentication;

namespace Ordivo.Infrastructure.Authentication;

internal sealed class IdentityTokenGenerator(TimeProvider timeProvider) : IIdentityTokenGenerator
{
    public GeneratedIdentityToken Generate(TimeSpan lifetime)
    {
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));
        return new GeneratedIdentityToken(token, Hash(token), timeProvider.GetUtcNow().Add(lifetime));
    }

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
