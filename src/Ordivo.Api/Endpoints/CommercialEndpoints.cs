using System.Security.Cryptography;
using System.Text;
using Carter;
using Ordivo.Api.Common;
using Ordivo.Application.Commercial;
using Ordivo.Domain.Commercial;
using Ordivo.SharedKernel.Messaging;
using Ordivo.Application.Abstractions.Payments;

namespace Ordivo.Api.Endpoints;

public sealed class CommercialEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/plans", async (IQueryHandler<ListPlansQuery, IReadOnlyCollection<PlanDto>> h, CancellationToken ct) => (await h.Handle(new(true), ct)).ToHttpResult()).WithTags("Commercial").AllowAnonymous();
        app.MapGet("/api/platform/plans", async (IQueryHandler<ListPlansQuery, IReadOnlyCollection<PlanDto>> h, CancellationToken ct) => (await h.Handle(new(false), ct)).ToHttpResult()).WithTags("Platform Commercial").RequireAuthorization("PlatformAdmin");
        app.MapPost("/api/platform/plans", async (PlanRequest r, ICommandHandler<UpsertPlanCommand, PlanDto> h, CancellationToken ct) => (await h.Handle(r.ToCommand(null), ct)).ToHttpResult()).WithTags("Platform Commercial").RequireAuthorization("PlatformAdmin");
        app.MapPut("/api/platform/plans/{id:guid}", async (Guid id, PlanRequest r, ICommandHandler<UpsertPlanCommand, PlanDto> h, CancellationToken ct) => (await h.Handle(r.ToCommand(id), ct)).ToHttpResult()).WithTags("Platform Commercial").RequireAuthorization("PlatformAdmin");
        app.MapPatch("/api/platform/plans/{id:guid}/status", async (Guid id, PlanStatusRequest r, ICommandHandler<ChangePlanStatusCommand, PlanDto> h, CancellationToken ct) => (await h.Handle(new(id, r.IsActive), ct)).ToHttpResult()).WithTags("Platform Commercial").RequireAuthorization("PlatformAdmin");
        app.MapPut("/api/platform/tenants/{tenantId:guid}/subscription", async (Guid tenantId, AssignSubscriptionRequest r, ICommandHandler<AssignSubscriptionCommand, SubscriptionDto> h, CancellationToken ct) => (await h.Handle(new(tenantId, r.PlanId, r.GatewayCustomerId, r.GatewaySubscriptionId), ct)).ToHttpResult()).WithTags("Platform Commercial").RequireAuthorization("PlatformAdmin");
        app.MapGet("/api/billing/subscription", async (IQueryHandler<GetCurrentSubscriptionQuery, SubscriptionDto> h, CancellationToken ct) => (await h.Handle(new(), ct)).ToHttpResult()).WithTags("Billing").RequireAuthorization();
        app.MapGet("/api/billing/invoices", async (IQueryHandler<ListInvoicesQuery, IReadOnlyCollection<InvoiceDto>> h, CancellationToken ct) => (await h.Handle(new(), ct)).ToHttpResult()).WithTags("Billing").RequireAuthorization();
        app.MapPost("/api/billing/checkout", async (CheckoutRequestBody r, ICommandHandler<CreateCheckoutCommand, CheckoutResult> h, CancellationToken ct) => (await h.Handle(new(r.SuccessUrl, r.CancelUrl), ct)).ToHttpResult()).WithTags("Billing").RequireAuthorization();
        app.MapPost("/api/webhooks/payments/{gateway}", HandleWebhook).WithTags("Payment Webhooks").AllowAnonymous();
    }
    private static async Task<IResult> HandleWebhook(string gateway, HttpRequest request, IConfiguration configuration, ICommandHandler<ProcessPaymentWebhookCommand, bool> handler, CancellationToken ct)
    {
        using var reader = new StreamReader(request.Body); var body = await reader.ReadToEndAsync(ct); var secret = configuration["Payments:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(secret)) return Results.Problem("Payment webhook secret is not configured.", statusCode: 503);
        if (!request.Headers.TryGetValue("X-Webhook-Signature", out var supplied)) return Results.Unauthorized();
        var expected = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        var signature = supplied.ToString().ToLowerInvariant();
        if (expected.Length != signature.Length || !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(signature))) return Results.Unauthorized();
        var payload = System.Text.Json.JsonSerializer.Deserialize<PaymentWebhookRequest>(body, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        if (payload is null) return Results.BadRequest();
        return (await handler.Handle(new(gateway.Trim().ToLowerInvariant(), payload.EventId, payload.Type, payload.TenantId, payload.InvoiceId, payload.Amount, payload.Currency ?? "BRL", payload.DueAt, payload.PeriodEnd), ct)).ToHttpResult();
    }
    public sealed record PlanRequest(string Name, string Code, decimal Price, string Currency, BillingInterval Interval, int TrialDays, int MaxUsers, int MaxCustomers, int MaxServiceOrders) { public UpsertPlanCommand ToCommand(Guid? id) => new(id, Name, Code, Price, Currency, Interval, TrialDays, MaxUsers, MaxCustomers, MaxServiceOrders); }
    public sealed record PlanStatusRequest(bool IsActive);
    public sealed record AssignSubscriptionRequest(Guid PlanId, string? GatewayCustomerId, string? GatewaySubscriptionId);
    public sealed record CheckoutRequestBody(string SuccessUrl, string CancelUrl);
    public sealed record PaymentWebhookRequest(string EventId, string Type, Guid TenantId, string? InvoiceId, decimal? Amount, string? Currency, DateTimeOffset? DueAt, DateTimeOffset? PeriodEnd);
}
