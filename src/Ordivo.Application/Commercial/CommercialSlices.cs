using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Application.Abstractions.Payments;
using Ordivo.Domain.Commercial;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Commercial;

public sealed record PlanDto(Guid Id, string Name, string Code, decimal Price, string Currency, BillingInterval Interval, int TrialDays, int MaxUsers, int MaxCustomers, int MaxServiceOrders, bool IsActive, int ActiveSubscriptions = 0);
public sealed record SubscriptionDto(Guid Id, Guid TenantId, PlanDto Plan, SubscriptionStatus Status, DateTimeOffset StartedAt, DateTimeOffset? TrialEndsAt, DateTimeOffset PeriodStartsAt, DateTimeOffset PeriodEndsAt, bool AccessBlocked, int UsersUsed, int CustomersUsed, int ServiceOrdersUsed);
public sealed record InvoiceDto(Guid Id, string GatewayInvoiceId, decimal Amount, string Currency, InvoiceStatus Status, DateTimeOffset DueAt, DateTimeOffset? PaidAt);
public static class CommercialMappings
{
    public static PlanDto ToDto(this Plan x, int activeSubscriptions = 0) => new(x.Id, x.Name, x.Code, x.Price, x.Currency, x.Interval, x.TrialDays, x.MaxUsers, x.MaxCustomers, x.MaxServiceOrders, x.IsActive, activeSubscriptions);
    public static PlanDto ToContractDto(this Subscription x) => new(x.PlanId, x.PlanName, x.PlanCode, x.ContractPrice, x.ContractCurrency, x.ContractInterval, x.ContractTrialDays, x.ContractMaxUsers, x.ContractMaxCustomers, x.ContractMaxServiceOrders, true);
    public static InvoiceDto ToDto(this BillingInvoice x) => new(x.Id, x.GatewayInvoiceId, x.Amount, x.Currency, x.Status, x.DueAt, x.PaidAt);
}

public sealed record ListPlansQuery(bool ActiveOnly = true) : IQuery<IReadOnlyCollection<PlanDto>>;
public sealed class ListPlansQueryHandler(ICommercialRepository repository) : IQueryHandler<ListPlansQuery, IReadOnlyCollection<PlanDto>>
{
    public async Task<Result<IReadOnlyCollection<PlanDto>>> Handle(ListPlansQuery q, CancellationToken ct)
    {
        var plans = await repository.ListPlansAsync(q.ActiveOnly, ct); var result = new List<PlanDto>(plans.Count);
        foreach (var plan in plans) result.Add(plan.ToDto(await repository.CountSubscriptionsByPlanAsync(plan.Id, ct)));
        return Result.Success<IReadOnlyCollection<PlanDto>>(result);
    }
}

public sealed record UpsertPlanCommand(Guid? Id, string Name, string Code, decimal Price, string Currency, BillingInterval Interval, int TrialDays, int MaxUsers, int MaxCustomers, int MaxServiceOrders) : ICommand<PlanDto>;
public sealed class UpsertPlanValidator : AbstractValidator<UpsertPlanCommand>
{
    public UpsertPlanValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120); RuleFor(x => x.Code).NotEmpty().MaximumLength(60).Matches("^[a-zA-Z0-9-]+$");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.Currency).Length(3); RuleFor(x => x.TrialDays).InclusiveBetween(0, 365);
        RuleFor(x => x.MaxUsers).GreaterThan(0); RuleFor(x => x.MaxCustomers).GreaterThan(0); RuleFor(x => x.MaxServiceOrders).GreaterThan(0);
    }
}
public sealed class UpsertPlanCommandHandler(ICommercialRepository repository, IUnitOfWork uow) : ICommandHandler<UpsertPlanCommand, PlanDto>
{
    public async Task<Result<PlanDto>> Handle(UpsertPlanCommand c, CancellationToken ct)
    {
        if (await repository.PlanCodeExistsAsync(c.Code, c.Id, ct)) return Result.Failure<PlanDto>(Error.Conflict("Plan code already exists."));
        Plan plan;
        if (c.Id.HasValue)
        {
            plan = await repository.GetPlanAsync(c.Id.Value, ct) ?? null!;
            if (plan is null) return Result.Failure<PlanDto>(Error.NotFound("Plan not found."));
            plan.Update(c.Name, c.Code, c.Price, c.Currency, c.Interval, c.TrialDays, c.MaxUsers, c.MaxCustomers, c.MaxServiceOrders);
        }
        else
        {
            plan = Plan.Create(c.Name, c.Code, c.Price, c.Currency, c.Interval, c.TrialDays, c.MaxUsers, c.MaxCustomers, c.MaxServiceOrders);
            await repository.AddPlanAsync(plan, ct);
        }
        await uow.SaveChangesAsync(ct); return Result.Success(plan.ToDto());
    }
}
public sealed record ChangePlanStatusCommand(Guid PlanId, bool IsActive) : ICommand<PlanDto>;
public sealed class ChangePlanStatusHandler(ICommercialRepository repository, IUnitOfWork uow) : ICommandHandler<ChangePlanStatusCommand, PlanDto>
{
    public async Task<Result<PlanDto>> Handle(ChangePlanStatusCommand c, CancellationToken ct) { var p = await repository.GetPlanAsync(c.PlanId, ct); if (p is null) return Result.Failure<PlanDto>(Error.NotFound("Plan not found.")); p.SetActive(c.IsActive); await uow.SaveChangesAsync(ct); return Result.Success(p.ToDto()); }
}

public sealed record AssignSubscriptionCommand(Guid TenantId, Guid PlanId, string? GatewayCustomerId, string? GatewaySubscriptionId) : ICommand<SubscriptionDto>;
public sealed class AssignSubscriptionHandler(ICommercialRepository repository, IPlatformTenantRepository tenants, IUnitOfWork uow, TimeProvider clock) : ICommandHandler<AssignSubscriptionCommand, SubscriptionDto>
{
    public async Task<Result<SubscriptionDto>> Handle(AssignSubscriptionCommand c, CancellationToken ct)
    {
        if (await tenants.GetAsync(c.TenantId, ct) is null) return Result.Failure<SubscriptionDto>(Error.NotFound("Tenant not found."));
        var plan = await repository.GetPlanAsync(c.PlanId, ct); if (plan is null || !plan.IsActive) return Result.Failure<SubscriptionDto>(Error.NotFound("Active plan not found."));
        var now = clock.GetUtcNow(); var subscription = await repository.GetSubscriptionAsync(c.TenantId, true, ct);
        if (subscription is null) { subscription = Subscription.Start(c.TenantId, plan, now, c.GatewayCustomerId, c.GatewaySubscriptionId); await repository.AddSubscriptionAsync(subscription, ct); }
        else { subscription.ChangePlan(plan, now); if (c.GatewayCustomerId is not null || c.GatewaySubscriptionId is not null) subscription.SetGatewayReferences(c.GatewayCustomerId, c.GatewaySubscriptionId); }
        await uow.SaveChangesAsync(ct); return Result.Success(await Build(subscription, repository, now, ct));
    }
    internal static async Task<SubscriptionDto> Build(Subscription s, ICommercialRepository r, DateTimeOffset now, CancellationToken ct) => new(s.Id, s.TenantId, s.ToContractDto(), s.Status, s.StartedAt, s.TrialEndsAt, s.CurrentPeriodStartsAt, s.CurrentPeriodEndsAt, s.BlocksAccess(now), await r.CountUsersAsync(s.TenantId, ct), await r.CountCustomersAsync(s.TenantId, ct), await r.CountServiceOrdersAsync(s.TenantId, s.CurrentPeriodStartsAt, ct));
}
public sealed record GetCurrentSubscriptionQuery : IQuery<SubscriptionDto>;
public sealed class GetCurrentSubscriptionHandler(ICommercialRepository repository, IUserContext user, TimeProvider clock) : IQueryHandler<GetCurrentSubscriptionQuery, SubscriptionDto>
{
    public async Task<Result<SubscriptionDto>> Handle(GetCurrentSubscriptionQuery q, CancellationToken ct) { var s = await repository.GetSubscriptionAsync(user.TenantId, false, ct); return s is null ? Result.Failure<SubscriptionDto>(Error.NotFound("Subscription not found.")) : Result.Success(await AssignSubscriptionHandler.Build(s, repository, clock.GetUtcNow(), ct)); }
}
public sealed record ListInvoicesQuery : IQuery<IReadOnlyCollection<InvoiceDto>>;
public sealed class ListInvoicesHandler(ICommercialRepository repository, IUserContext user) : IQueryHandler<ListInvoicesQuery, IReadOnlyCollection<InvoiceDto>>
{
    public async Task<Result<IReadOnlyCollection<InvoiceDto>>> Handle(ListInvoicesQuery q, CancellationToken ct) => Result.Success<IReadOnlyCollection<InvoiceDto>>((await repository.ListInvoicesAsync(user.TenantId, ct)).Select(x => x.ToDto()).ToArray());
}

public sealed record CreateCheckoutCommand(string SuccessUrl, string CancelUrl) : ICommand<CheckoutResult>;
public sealed class CreateCheckoutValidator : AbstractValidator<CreateCheckoutCommand>
{
    public CreateCheckoutValidator() { RuleFor(x => x.SuccessUrl).NotEmpty().Must(x => Uri.TryCreate(x, UriKind.Absolute, out _)); RuleFor(x => x.CancelUrl).NotEmpty().Must(x => Uri.TryCreate(x, UriKind.Absolute, out _)); }
}
public sealed class CreateCheckoutHandler(ICommercialRepository repository, IPaymentGateway gateway, IUserContext user) : ICommandHandler<CreateCheckoutCommand, CheckoutResult>
{
    public async Task<Result<CheckoutResult>> Handle(CreateCheckoutCommand c, CancellationToken ct)
    {
        var subscription = await repository.GetSubscriptionAsync(user.TenantId, false, ct); if (subscription is null) return Result.Failure<CheckoutResult>(Error.NotFound("Subscription not found."));
        var checkout = await gateway.CreateCheckoutAsync(new(user.TenantId, subscription.Id, subscription.PlanId, subscription.ContractPrice, subscription.ContractCurrency, subscription.PlanName, c.SuccessUrl, c.CancelUrl), ct);
        return Result.Success(checkout);
    }
}

public sealed record ProcessPaymentWebhookCommand(string Gateway, string EventId, string Type, Guid TenantId, string? InvoiceId, decimal? Amount, string Currency, DateTimeOffset? DueAt, DateTimeOffset? PeriodEnd) : ICommand<bool>;
public sealed class ProcessPaymentWebhookHandler(ICommercialRepository repository, IUnitOfWork uow, TimeProvider clock) : ICommandHandler<ProcessPaymentWebhookCommand, bool>
{
    public async Task<Result<bool>> Handle(ProcessPaymentWebhookCommand c, CancellationToken ct)
    {
        if (await repository.WebhookExistsAsync(c.Gateway, c.EventId, ct)) return Result.Success(true);
        var now = clock.GetUtcNow(); var webhook = PaymentWebhookEvent.Receive(c.Gateway, c.EventId, c.Type, System.Text.Json.JsonSerializer.Serialize(c), now); await repository.AddWebhookAsync(webhook, ct);
        var subscription = await repository.GetSubscriptionAsync(c.TenantId, true, ct); if (subscription is null) return Result.Failure<bool>(Error.NotFound("Subscription not found."));
        BillingInvoice? invoice = null;
        if (!string.IsNullOrWhiteSpace(c.InvoiceId)) { invoice = await repository.GetInvoiceByGatewayIdAsync(c.InvoiceId, ct); if (invoice is null && c.Amount.HasValue && c.DueAt.HasValue) { invoice = BillingInvoice.Create(c.TenantId, subscription.Id, c.InvoiceId, c.Amount.Value, c.Currency, c.DueAt.Value); await repository.AddInvoiceAsync(invoice, ct); } }
        switch (c.Type.Trim().ToLowerInvariant())
        {
            case "invoice.paid": invoice?.MarkPaid(now); subscription.MarkActive(c.PeriodEnd ?? subscription.CurrentPeriodEndsAt, now); break;
            case "invoice.failed": invoice?.MarkFailed(); subscription.MarkPastDue(); break;
            case "subscription.suspended": subscription.Suspend(); break;
            case "subscription.canceled": subscription.Cancel(now); break;
            default: return Result.Failure<bool>(Error.Validation("Unsupported payment event type."));
        }
        webhook.MarkProcessed(now); await uow.SaveChangesAsync(ct); return Result.Success(true);
    }
}
