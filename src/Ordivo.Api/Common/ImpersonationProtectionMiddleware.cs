namespace Ordivo.Api.Common;
public sealed class ImpersonationProtectionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var impersonating=context.User.HasClaim(c=>c.Type=="impersonation_session_id");
        if(impersonating && IsUnsafe(context.Request.Method) && IsSensitive(context.Request.Path))
        {
            context.Response.StatusCode=StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new{type="https://httpstatuses.com/403",title="Blocked during impersonation",status=403,detail="This sensitive operation is not allowed while impersonating a tenant user.",instance=context.Request.Path.Value,traceId=context.TraceIdentifier});return;
        }
        await next(context);
    }
    private static bool IsUnsafe(string method)=>!HttpMethods.IsGet(method)&&!HttpMethods.IsHead(method)&&!HttpMethods.IsOptions(method);
    private static bool IsSensitive(PathString path)=>path.StartsWithSegments("/api/billing")||path.StartsWithSegments("/api/auth/sessions")||path.StartsWithSegments("/api/auth/refresh")||path.StartsWithSegments("/api/auth/logout")||path.StartsWithSegments("/api/platform/auth/refresh")||path.StartsWithSegments("/api/users/me/password")||path.StartsWithSegments("/api/tenant")||path.StartsWithSegments("/api/users");
}
