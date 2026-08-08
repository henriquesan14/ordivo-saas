using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Ordivo.Application.Abstractions.Payments;

namespace Ordivo.Infrastructure.Payments;

public sealed class PaymentOptions
{
    public const string SectionName = "Payments";
    public string ApiBaseUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
}
internal sealed class HttpPaymentGateway(HttpClient client, IOptions<PaymentOptions> options) : IPaymentGateway
{
    public async Task<CheckoutResult> CreateCheckoutAsync(CheckoutRequest request, CancellationToken ct)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiBaseUrl) || string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new InvalidOperationException("Payment gateway is not configured.");
        var payload = new
        {
            billingTypes = new[] { "CREDIT_CARD" }, chargeTypes = new[] { "RECURRENT" }, minutesToExpire = 60,
            externalReference = request.TenantId.ToString(),
            callback = new { successUrl = request.SuccessUrl, cancelUrl = request.CancelUrl, expiredUrl = request.CancelUrl },
            items = new[] { new { externalReference = request.PlanId.ToString(), name = request.Description, description = $"Assinatura {request.Description}", quantity = 1, value = request.Amount } },
            customerData = string.IsNullOrWhiteSpace(request.CustomerName) && string.IsNullOrWhiteSpace(request.CustomerEmail) ? null : new { name = request.CustomerName, email = request.CustomerEmail },
            subscription = new { cycle = request.Cycle, nextDueDate = request.FirstDueDate.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss") }
        };
        using var message = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(settings.ApiBaseUrl.TrimEnd('/') + "/"), "checkouts"));
        message.Headers.TryAddWithoutValidation("access_token", settings.ApiKey);
        message.Headers.UserAgent.ParseAdd("Ordivo/1.0");
        message.Content = JsonContent.Create(payload);
        using var response = await client.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Asaas checkout failed ({(int)response.StatusCode}): {error[..Math.Min(error.Length, 2000)]}");
        }
        var result = await response.Content.ReadFromJsonAsync<AsaasCheckoutResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Asaas returned an invalid checkout response.");
        if (string.IsNullOrWhiteSpace(result.Id) || string.IsNullOrWhiteSpace(result.Link))
            throw new InvalidOperationException("Asaas checkout response does not contain id and link.");
        return new CheckoutResult(result.Id, result.Link);
    }
    private sealed record AsaasCheckoutResponse(string Id, string Link, string Status);
}
