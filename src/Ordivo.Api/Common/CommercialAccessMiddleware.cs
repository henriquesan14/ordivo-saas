using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;

namespace Ordivo.Api.Common;

public sealed class CommercialAccessMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICommercialRepository commercial, IUserContext user, TimeProvider clock)
    {
        if (!user.IsAuthenticated || IsExcluded(context.Request.Path)) { await next(context); return; }
        if (user.TenantId == Guid.Empty) { await next(context); return; }
        var subscription = await commercial.GetSubscriptionAsync(user.TenantId, false, context.RequestAborted);
        if (subscription is null) { await next(context); return; }
        if (subscription.BlocksAccess(clock.GetUtcNow())) { await WriteProblem(context, StatusCodes.Status402PaymentRequired, "Subscription blocked", "The tenant subscription is past due, suspended, canceled, or its trial expired."); return; }
        if (HttpMethods.IsPost(context.Request.Method))
        {
            var exceeded = context.Request.Path.Value?.ToLowerInvariant() switch
            {
                "/api/users" => await commercial.CountUsersAsync(user.TenantId, context.RequestAborted) >= subscription.ContractMaxUsers,
                "/api/customers" => await commercial.CountCustomersAsync(user.TenantId, context.RequestAborted) >= subscription.ContractMaxCustomers,
                "/api/service-orders" => await commercial.CountServiceOrdersAsync(user.TenantId, subscription.CurrentPeriodStartsAt, context.RequestAborted) >= subscription.ContractMaxServiceOrders,
                _ => false
            };
            if (exceeded) { await WriteProblem(context, StatusCodes.Status403Forbidden, "Plan limit reached", "Upgrade the subscription plan to create more resources."); return; }
        }
        await next(context);
    }
    private static bool IsExcluded(PathString path) => path.StartsWithSegments("/api/auth") || path.StartsWithSegments("/api/platform") || path.StartsWithSegments("/api/impersonation") || path.StartsWithSegments("/api/billing") || path.StartsWithSegments("/health");
    private static Task WriteProblem(HttpContext context, int status, string title, string detail) { context.Response.StatusCode = status; return context.Response.WriteAsJsonAsync(new { type = $"https://httpstatuses.com/{status}", title, status, detail, instance = context.Request.Path.Value, traceId = context.TraceIdentifier }); }
}
