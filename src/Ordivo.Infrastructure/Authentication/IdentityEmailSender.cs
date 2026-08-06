using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ordivo.Application.Abstractions.Authentication;

namespace Ordivo.Infrastructure.Authentication;

public sealed class EmailOptions
{
    public const string SectionName = "Email";
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromEmail { get; init; } = "no-reply@ordivo.local";
    public string FromName { get; init; } = "Ordivo";
    public string FrontendBaseUrl { get; init; } = "http://localhost:4200";
}

internal sealed class IdentityEmailSender(
    IOptions<EmailOptions> options,
    ILogger<IdentityEmailSender> logger) : IIdentityEmailSender
{
    public Task SendEmailVerificationAsync(string email, string name, string token, CancellationToken ct) =>
        SendAsync(email, name, "Verify your Ordivo email", "verify-email", token, ct);

    public Task SendPasswordResetAsync(string email, string name, string token, CancellationToken ct) =>
        SendAsync(email, name, "Reset your Ordivo password", "reset-password", token, ct);

    public Task SendUserInvitationAsync(string email, string name, string token, CancellationToken ct) =>
        SendAsync(email, name, "You were invited to Ordivo", "accept-invitation", token, ct);

    private async Task SendAsync(
        string email,
        string name,
        string subject,
        string route,
        string token,
        CancellationToken ct)
    {
        var settings = options.Value;
        var link = $"{settings.FrontendBaseUrl.TrimEnd('/')}/{route}?token={Uri.EscapeDataString(token)}";
        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            logger.LogWarning("Email delivery is not configured. {Subject} for {Email}: {Link}", subject, email, link);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromEmail, settings.FromName),
            Subject = subject,
            Body = $"Hello {WebUtility.HtmlEncode(name)},<br><br><a href=\"{WebUtility.HtmlEncode(link)}\">Continue in Ordivo</a>",
            IsBodyHtml = true
        };
        message.To.Add(email);
        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.UseSsl,
            Credentials = string.IsNullOrWhiteSpace(settings.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(settings.Username, settings.Password)
        };
        await client.SendMailAsync(message, ct);
    }
}
