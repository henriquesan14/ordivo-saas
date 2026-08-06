using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Ordivo.Infrastructure.Authentication;
using Ordivo.Infrastructure.Persistence;

namespace Ordivo.Infrastructure.BackgroundJobs;
internal sealed class OutboxWorker(IServiceScopeFactory scopes,ILogger<OutboxWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(5));
        do { await Process(stoppingToken); } while(await timer.WaitForNextTickAsync(stoppingToken));
    }
    private async Task Process(CancellationToken ct)
    {
        await using var scope=scopes.CreateAsyncScope(); var db=scope.ServiceProvider.GetRequiredService<OrdivoDbContext>();
        var now=DateTimeOffset.UtcNow; var messages=await db.OutboxMessages.Where(x=>x.Type=="email"&&x.ProcessedAt==null&&(x.NextAttemptAt==null||x.NextAttemptAt<=now)).OrderBy(x=>x.OccurredAt).Take(20).ToListAsync(ct);
        var options=scope.ServiceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;
        foreach(var message in messages) try { var p=JsonSerializer.Deserialize<EmailOutboxPayload>(message.Payload)!; await Send(options,p,logger,ct); message.ProcessedAt=now; message.Error=null; }
        catch(Exception ex){message.Attempts++;message.Error=ex.Message[..Math.Min(ex.Message.Length,4000)];message.NextAttemptAt=now.AddMinutes(Math.Min(60,Math.Pow(2,message.Attempts)));logger.LogError(ex,"Outbox delivery failed for {OutboxId}",message.Id);}
        await db.SaveChangesAsync(ct);
    }
    private static async Task Send(EmailOptions o,EmailOutboxPayload p,ILogger logger,CancellationToken ct)
    {
        var link=$"{o.FrontendBaseUrl.TrimEnd('/')}/{p.Route}?token={Uri.EscapeDataString(p.Token)}";
        if(string.IsNullOrWhiteSpace(o.Host)){logger.LogWarning("Email delivery is not configured. {Subject} for {Email}: {Link}",p.Subject,p.Email,link);return;}
        using var m=new MailMessage{From=new MailAddress(o.FromEmail,o.FromName),Subject=p.Subject,Body=$"Hello {WebUtility.HtmlEncode(p.Name)},<br><br><a href=\"{WebUtility.HtmlEncode(link)}\">Continue in Ordivo</a>",IsBodyHtml=true};m.To.Add(p.Email);
        using var client=new SmtpClient(o.Host,o.Port){EnableSsl=o.UseSsl,Credentials=string.IsNullOrWhiteSpace(o.Username)?CredentialCache.DefaultNetworkCredentials:new NetworkCredential(o.Username,o.Password)};await client.SendMailAsync(m,ct);
    }
}
internal sealed class SessionCleanupWorker(IServiceScopeFactory scopes,ILogger<SessionCleanupWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct){using var timer=new PeriodicTimer(TimeSpan.FromHours(1));do{await using var s=scopes.CreateAsyncScope();var db=s.ServiceProvider.GetRequiredService<OrdivoDbContext>();var cutoff=DateTimeOffset.UtcNow.AddDays(-7);var count=await db.AuthSessions.Where(x=>x.ExpiresAt<cutoff).ExecuteDeleteAsync(ct);if(count>0)logger.LogInformation("Removed {Count} expired auth sessions",count);}while(await timer.WaitForNextTickAsync(ct));}
}
