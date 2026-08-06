using Ordivo.SharedKernel.Domain;

namespace Ordivo.Domain.Commercial;

public enum BillingInterval { Monthly, Yearly }
public enum SubscriptionStatus { Trialing, Active, PastDue, Suspended, Canceled }
public enum InvoiceStatus { Pending, Paid, Failed, Refunded, Canceled }

public sealed class Plan : AggregateRoot<Guid>
{
    private Plan(Guid id) : base(id) { }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public BillingInterval Interval { get; private set; }
    public int TrialDays { get; private set; }
    public int MaxUsers { get; private set; }
    public int MaxCustomers { get; private set; }
    public int MaxServiceOrders { get; private set; }
    public bool IsActive { get; private set; }

    public static Plan Create(string name, string code, decimal price, string currency, BillingInterval interval,
        int trialDays, int maxUsers, int maxCustomers, int maxServiceOrders) =>
        new Plan(Guid.NewGuid()) { IsActive = true }.Update(name, code, price, currency, interval, trialDays, maxUsers, maxCustomers, maxServiceOrders);

    public Plan Update(string name, string code, decimal price, string currency, BillingInterval interval,
        int trialDays, int maxUsers, int maxCustomers, int maxServiceOrders)
    {
        Name = name.Trim(); Code = code.Trim().ToLowerInvariant(); Price = price;
        Currency = currency.Trim().ToUpperInvariant(); Interval = interval; TrialDays = trialDays;
        MaxUsers = maxUsers; MaxCustomers = maxCustomers; MaxServiceOrders = maxServiceOrders;
        return this;
    }
    public void SetActive(bool active) => IsActive = active;
}

public sealed class Subscription : AggregateRoot<Guid>
{
    private Subscription(Guid id) : base(id) { }
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset CurrentPeriodStartsAt { get; private set; }
    public DateTimeOffset? TrialEndsAt { get; private set; }
    public DateTimeOffset CurrentPeriodEndsAt { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    public string? GatewayCustomerId { get; private set; }
    public string? GatewaySubscriptionId { get; private set; }

    public static Subscription Start(Guid tenantId, Plan plan, DateTimeOffset now, string? customerId = null, string? subscriptionId = null)
    {
        var trialEnd = plan.TrialDays > 0 ? now.AddDays(plan.TrialDays) : (DateTimeOffset?)null;
        return new Subscription(Guid.NewGuid())
        {
            TenantId = tenantId, PlanId = plan.Id, StartedAt = now, TrialEndsAt = trialEnd,
            CurrentPeriodStartsAt = now, CurrentPeriodEndsAt = trialEnd ?? AddPeriod(now, plan.Interval),
            Status = trialEnd.HasValue ? SubscriptionStatus.Trialing : SubscriptionStatus.Active,
            GatewayCustomerId = customerId, GatewaySubscriptionId = subscriptionId
        };
    }

    public void ChangePlan(Plan plan, DateTimeOffset now) { PlanId = plan.Id; CurrentPeriodStartsAt = now; CurrentPeriodEndsAt = AddPeriod(now, plan.Interval); }
    public void SetGatewayReferences(string? customerId, string? subscriptionId) { GatewayCustomerId = customerId; GatewaySubscriptionId = subscriptionId; }
    public void MarkActive(DateTimeOffset periodEnd, DateTimeOffset now) { Status = SubscriptionStatus.Active; CurrentPeriodStartsAt = now; CurrentPeriodEndsAt = periodEnd; TrialEndsAt = null; }
    public void MarkPastDue() => Status = SubscriptionStatus.PastDue;
    public void Suspend() => Status = SubscriptionStatus.Suspended;
    public void Cancel(DateTimeOffset now) { Status = SubscriptionStatus.Canceled; CanceledAt = now; }
    public bool BlocksAccess(DateTimeOffset now) => Status is SubscriptionStatus.PastDue or SubscriptionStatus.Suspended or SubscriptionStatus.Canceled || Status == SubscriptionStatus.Trialing && TrialEndsAt <= now;
    private static DateTimeOffset AddPeriod(DateTimeOffset now, BillingInterval interval) => interval == BillingInterval.Yearly ? now.AddYears(1) : now.AddMonths(1);
}

public sealed class BillingInvoice : AggregateRoot<Guid>
{
    private BillingInvoice(Guid id) : base(id) { }
    public Guid TenantId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public string GatewayInvoiceId { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset DueAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public static BillingInvoice Create(Guid tenantId, Guid subscriptionId, string gatewayId, decimal amount, string currency, DateTimeOffset dueAt) =>
        new(Guid.NewGuid()) { TenantId = tenantId, SubscriptionId = subscriptionId, GatewayInvoiceId = gatewayId, Amount = amount, Currency = currency, DueAt = dueAt, Status = InvoiceStatus.Pending };
    public void MarkPaid(DateTimeOffset now) { Status = InvoiceStatus.Paid; PaidAt = now; }
    public void MarkFailed() => Status = InvoiceStatus.Failed;
}

public sealed class PaymentWebhookEvent
{
    private PaymentWebhookEvent() { }
    public Guid Id { get; private set; }
    public string Gateway { get; private set; } = string.Empty;
    public string ExternalEventId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public static PaymentWebhookEvent Receive(string gateway, string eventId, string type, string payload, DateTimeOffset now) =>
        new() { Id = Guid.NewGuid(), Gateway = gateway, ExternalEventId = eventId, EventType = type, Payload = payload, ReceivedAt = now };
    public void MarkProcessed(DateTimeOffset now) => ProcessedAt = now;
}
