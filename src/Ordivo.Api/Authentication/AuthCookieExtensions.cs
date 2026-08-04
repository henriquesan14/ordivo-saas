using Microsoft.Extensions.Options;
using Ordivo.Application.Authentication;
using Ordivo.Application.Platform.Authentication;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Api.Authentication;

public static class AuthCookieExtensions
{
    public static IResult ToAuthCookieResult(this Result<AuthDto> result, HttpContext context) =>
        result.IsSuccess
            ? SignIn(context, result.Value.AccessToken, result.Value.ExpiresAt, new
            {
                result.Value.UserId,
                result.Value.TenantId,
                result.Value.Name,
                result.Value.Email,
                result.Value.Role,
                result.Value.ExpiresAt
            })
            : ToErrorResult(result.Error);

    public static IResult ToAuthCookieResult(this Result<PlatformAuthDto> result, HttpContext context) =>
        result.IsSuccess
            ? SignIn(context, result.Value.AccessToken, result.Value.ExpiresAt, new
            {
                result.Value.UserId,
                result.Value.Name,
                result.Value.Email,
                result.Value.Role,
                result.Value.ExpiresAt
            })
            : ToErrorResult(result.Error);

    public static void DeleteAuthCookie(this HttpContext context)
    {
        var settings = context.RequestServices.GetRequiredService<IOptions<AuthCookieOptions>>().Value;
        context.Response.Cookies.Delete(settings.Name, CreateCookieOptions(settings, DateTimeOffset.UnixEpoch));
    }

    private static IResult SignIn(HttpContext context, string token, DateTimeOffset expiresAt, object response)
    {
        var settings = context.RequestServices.GetRequiredService<IOptions<AuthCookieOptions>>().Value;
        context.Response.Cookies.Append(settings.Name, token, CreateCookieOptions(settings, expiresAt));
        return Results.Ok(response);
    }

    private static CookieOptions CreateCookieOptions(AuthCookieOptions settings, DateTimeOffset expiresAt) => new()
    {
        HttpOnly = true,
        Secure = settings.Secure,
        SameSite = settings.SameSite,
        Expires = expiresAt,
        IsEssential = true,
        Path = "/"
    };

    private static IResult ToErrorResult(Error error) => error.Code switch
    {
        "validation" => Results.BadRequest(new { error }),
        "not_found" => Results.NotFound(new { error }),
        "conflict" => Results.Conflict(new { error }),
        "unauthorized" => Results.Json(new { error }, statusCode: StatusCodes.Status401Unauthorized),
        _ => Results.Problem(error.Description)
    };
}
