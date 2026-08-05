using Microsoft.Extensions.Options;
using Ordivo.Application.Authentication;
using Ordivo.Application.Platform.Authentication;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Api.Authentication;

public static class AuthCookieExtensions
{
    public static IResult ToAuthCookieResult(this Result<AuthDto> result, HttpContext context) =>
        result.IsSuccess
            ? SignIn(context, result.Value.AccessToken, result.Value.ExpiresAt,
                result.Value.RefreshToken, result.Value.RefreshExpiresAt, new
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
            ? SignIn(context, result.Value.AccessToken, result.Value.ExpiresAt,
                result.Value.RefreshToken, result.Value.RefreshExpiresAt, new
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
        context.Response.Cookies.Delete(settings.RefreshName, CreateCookieOptions(settings, DateTimeOffset.UnixEpoch, "/api"));
    }

    public static string? GetRefreshToken(this HttpContext context)
    {
        var settings = context.RequestServices.GetRequiredService<IOptions<AuthCookieOptions>>().Value;
        return context.Request.Cookies[settings.RefreshName];
    }

    private static IResult SignIn(
        HttpContext context,
        string token,
        DateTimeOffset expiresAt,
        string refreshToken,
        DateTimeOffset refreshExpiresAt,
        object response)
    {
        var settings = context.RequestServices.GetRequiredService<IOptions<AuthCookieOptions>>().Value;
        context.Response.Cookies.Append(settings.Name, token, CreateCookieOptions(settings, expiresAt));
        context.Response.Cookies.Append(
            settings.RefreshName,
            refreshToken,
            CreateCookieOptions(settings, refreshExpiresAt, "/api"));
        return Results.Ok(response);
    }

    private static CookieOptions CreateCookieOptions(
        AuthCookieOptions settings,
        DateTimeOffset expiresAt,
        string path = "/") => new()
    {
        HttpOnly = true,
        Secure = settings.Secure,
        SameSite = settings.SameSite,
        Expires = expiresAt,
        IsEssential = true,
        Path = path
    };

    private static IResult ToErrorResult(Error error) => error.Code switch
    {
        "validation" => Results.BadRequest(new { error }),
        "not_found" => Results.NotFound(new { error }),
        "conflict" => Results.Conflict(new { error }),
        "forbidden" => Results.Json(new { error }, statusCode: StatusCodes.Status403Forbidden),
        "unauthorized" => Results.Json(new { error }, statusCode: StatusCodes.Status401Unauthorized),
        _ => Results.Problem(error.Description)
    };
}
