namespace Ordivo.Application.Abstractions.Payments;

public sealed record CheckoutRequest(Guid TenantId, Guid SubscriptionId, Guid PlanId, decimal Amount, string Currency, string Description, string SuccessUrl, string CancelUrl);
public sealed record CheckoutResult(string CheckoutId, string CheckoutUrl);
public interface IPaymentGateway
{
    Task<CheckoutResult> CreateCheckoutAsync(CheckoutRequest request, CancellationToken ct);
}
