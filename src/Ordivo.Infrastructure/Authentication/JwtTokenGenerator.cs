using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Domain.Users;
using Ordivo.Domain.PlatformUsers;

namespace Ordivo.Infrastructure.Authentication;

internal sealed class JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider) : IGenerateToken
{
    public AccessToken GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        return Generate(claims);
    }

    public AccessToken GenerateToken(PlatformUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim("platform_role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        return Generate(claims);
    }

    public AccessToken GenerateImpersonationToken(User user, Guid platformUserId, string platformUserName, Guid sessionId, DateTimeOffset expiresAt)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim("name", user.Name), new Claim("role", user.Role.ToString()),
            new Claim("impersonator_user_id", platformUserId.ToString()), new Claim("impersonator_name", platformUserName),
            new Claim("impersonation_session_id", sessionId.ToString()), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        return Generate(claims, expiresAt);
    }

    private AccessToken Generate(IEnumerable<Claim> claims, DateTimeOffset? fixedExpiration = null)
    {
        var jwt = options.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = fixedExpiration ?? now.AddMinutes(jwt.AccessTokenExpirationMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, now.UtcDateTime, expiresAt.UtcDateTime, credentials);
        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
