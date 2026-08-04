using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Ordivo.Application.Abstractions.Authentication;

namespace Ordivo.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private System.Security.Claims.ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;
    public Guid UserId => IsAuthenticated && Guid.TryParse(Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id)
        ? id
        : throw new InvalidOperationException("The current request has no authenticated user identifier.");
    public Guid TenantId => IsAuthenticated && Guid.TryParse(Principal?.FindFirst("tenant_id")?.Value, out var id)
        ? id
        : throw new InvalidOperationException("The current request has no authenticated tenant identifier.");
    public string? Name => Principal?.FindFirst("name")?.Value;
    public string? Email => Principal?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    public string? Role => Principal?.FindFirst("role")?.Value;
}
