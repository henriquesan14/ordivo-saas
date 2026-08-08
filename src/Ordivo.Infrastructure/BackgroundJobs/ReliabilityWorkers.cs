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
        if(string.IsNullOrWhiteSpace(o.Host)) throw new InvalidOperationException("Email delivery is not configured.");
        var name=WebUtility.HtmlEncode(p.Name);var safeLink=WebUtility.HtmlEncode(link);
        var content=p.Route switch
        {
            "accept-invitation" => ("CONVITE PARA A EQUIPE","Você foi convidado para o Ordivo","Sua equipe convidou você para organizar clientes e ordens de serviço em um só lugar.","Aceitar convite e criar senha","7 dias"),
            "verify-email" => ("BEM-VINDO AO ORDIVO","Confirme seu e-mail","Sua empresa foi criada. Confirme seu endereço de e-mail para ativar o acesso ao Ordivo.","Confirmar meu e-mail","24 horas"),
            "reset-password" => ("SEGURANÇA DA CONTA","Redefina sua senha","Recebemos uma solicitação para criar uma nova senha para sua conta.","Criar nova senha","1 hora"),
            _ => ("SUA CONTA ORDIVO","Continue no Ordivo","Recebemos uma solicitação relacionada à sua conta.","Continuar no Ordivo","24 horas")
        };
        var body=$"""<!doctype html><html><body style="margin:0;background:#f4f6f1;font-family:Arial,sans-serif;color:#18322f"><table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:36px 16px"><tr><td align="center"><table role="presentation" width="560" cellspacing="0" cellpadding="0" style="max-width:560px;background:#fff;border:1px solid #e1e7e0;border-radius:16px"><tr><td style="padding:34px"><div style="font-size:22px;font-weight:700;color:#173f39;margin-bottom:38px"><span style="display:inline-block;background:#173f39;color:#e6ff72;border-radius:9px;padding:7px 11px;margin-right:8px;font-family:Georgia,serif;font-style:italic">O</span> Ordivo</div><p style="margin:0 0 8px;color:#64827d;font-size:11px;font-weight:700;letter-spacing:1.5px">{content.Item1}</p><h1 style="margin:0 0 14px;font-size:28px">{content.Item2}</h1><p style="margin:0 0 10px;color:#53635f;line-height:1.6">Olá, {name}.</p><p style="margin:0 0 26px;color:#53635f;line-height:1.6">{content.Item3}</p><a href="{safeLink}" style="display:inline-block;padding:14px 20px;background:#173f39;color:#fff;text-decoration:none;border-radius:9px;font-weight:700">{content.Item4} &nbsp;→</a><p style="margin:28px 0 0;color:#889592;font-size:12px;line-height:1.5">Este link é pessoal e expira em {content.Item5}. Se você não fez esta solicitação, ignore este e-mail.</p></td></tr></table></td></tr></table></body></html>""";
        using var m=new MailMessage{From=new MailAddress(o.FromEmail,o.FromName),Subject=content.Item2,Body=body,IsBodyHtml=true};m.To.Add(p.Email);
        using var client=new SmtpClient(o.Host,o.Port){EnableSsl=o.UseSsl,Credentials=string.IsNullOrWhiteSpace(o.Username)?CredentialCache.DefaultNetworkCredentials:new NetworkCredential(o.Username,o.Password)};await client.SendMailAsync(m,ct);
    }
}
internal sealed class SessionCleanupWorker(IServiceScopeFactory scopes,ILogger<SessionCleanupWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct){using var timer=new PeriodicTimer(TimeSpan.FromHours(1));do{await using var s=scopes.CreateAsyncScope();var db=s.ServiceProvider.GetRequiredService<OrdivoDbContext>();var cutoff=DateTimeOffset.UtcNow.AddDays(-7);var count=await db.AuthSessions.Where(x=>x.ExpiresAt<cutoff).ExecuteDeleteAsync(ct);if(count>0)logger.LogInformation("Removed {Count} expired auth sessions",count);}while(await timer.WaitForNextTickAsync(ct));}
}
