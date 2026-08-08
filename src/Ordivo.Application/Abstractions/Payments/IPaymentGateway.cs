namespace Ordivo.Application.Abstractions.Payments;

public sealed record CheckoutRequest(Guid TenantId, Guid SubscriptionId, Guid PlanId, decimal Amount, string Currency, string Description, string Cycle, DateTimeOffset FirstDueDate, string? CustomerName, string? CustomerEmail, string SuccessUrl, string CancelUrl);
public sealed record CheckoutResult(string CheckoutId, string CheckoutUrl);
public interface IPaymentGateway
{
    Task<CheckoutResult> CreateCheckoutAsync(CheckoutRequest request, CancellationToken ct);
}
