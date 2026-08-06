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
        using var message = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(settings.ApiBaseUrl.TrimEnd('/') + "/"), "checkouts"));
        message.Headers.Authorization = new("Bearer", settings.ApiKey); message.Content = JsonContent.Create(request);
        using var response = await client.SendAsync(message, ct); response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CheckoutResult>(cancellationToken: ct) ?? throw new InvalidOperationException("Payment gateway returned an invalid checkout response.");
    }
}
