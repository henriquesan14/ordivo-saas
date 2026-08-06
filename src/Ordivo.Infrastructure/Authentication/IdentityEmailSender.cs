using System.Text.Json;
using Microsoft.Extensions.Options;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Infrastructure.Persistence;

namespace Ordivo.Infrastructure.Authentication;
public sealed class EmailOptions
{
    public const string SectionName="Email"; public string Host{get;init;}=string.Empty; public int Port{get;init;}=587;
    public bool UseSsl{get;init;}=true; public string Username{get;init;}=string.Empty; public string Password{get;init;}=string.Empty;
    public string FromEmail{get;init;}="no-reply@ordivo.local"; public string FromName{get;init;}="Ordivo"; public string FrontendBaseUrl{get;init;}="http://localhost:4200";
}
internal sealed record EmailOutboxPayload(string Email,string Name,string Subject,string Route,string Token);
internal sealed class IdentityEmailSender(OrdivoDbContext db,TimeProvider time):IIdentityEmailSender
{
    public Task SendEmailVerificationAsync(string e,string n,string t,CancellationToken ct)=>Queue(e,n,"Verify your Ordivo email","verify-email",t);
    public Task SendPasswordResetAsync(string e,string n,string t,CancellationToken ct)=>Queue(e,n,"Reset your Ordivo password","reset-password",t);
    public Task SendUserInvitationAsync(string e,string n,string t,CancellationToken ct)=>Queue(e,n,"You were invited to Ordivo","accept-invitation",t);
    private Task Queue(string e,string n,string s,string r,string t){db.OutboxMessages.Add(new(){Type="email",Payload=JsonSerializer.Serialize(new EmailOutboxPayload(e,n,s,r,t)),OccurredAt=time.GetUtcNow()});return Task.CompletedTask;}
}
