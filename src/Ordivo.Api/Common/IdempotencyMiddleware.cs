using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Ordivo.Infrastructure.Persistence;

namespace Ordivo.Api.Common;
public sealed class IdempotencyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context,OrdivoDbContext db,TimeProvider time)
    {
        if(context.User.Identity?.IsAuthenticated != true || context.Request.Method is "GET" or "HEAD" or "OPTIONS" || context.Request.Path.StartsWithSegments("/api/auth") || context.Request.Path.StartsWithSegments("/api/platform/auth") || !context.Request.Headers.TryGetValue("Idempotency-Key",out var key)){await next(context);return;}
        var value=key.ToString(); if(string.IsNullOrWhiteSpace(value)||value.Length>200){context.Response.StatusCode=400;await context.Response.WriteAsJsonAsync(new{title="Invalid Idempotency-Key"});return;}
        var subject=context.User.FindFirstValue("sub")??"anonymous";var tenant=context.User.FindFirstValue("tenant_id")??"platform";var scope=$"{tenant}:{subject}:{context.Request.Method}:{context.Request.Path}";
        await using var transaction=await db.Database.BeginTransactionAsync(context.RequestAborted);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({scope + ":" + value}, 0))",context.RequestAborted);
        var existing=await db.IdempotencyRecords.AsNoTracking().SingleOrDefaultAsync(x=>x.Scope==scope&&x.Key==value&&x.ExpiresAt>time.GetUtcNow(),context.RequestAborted);
        if(existing is not null){context.Response.StatusCode=existing.StatusCode;context.Response.ContentType=existing.ContentType;context.Response.Headers["Idempotency-Replayed"]="true";await context.Response.WriteAsync(existing.ResponseBody,context.RequestAborted);await transaction.CommitAsync(context.RequestAborted);return;}
        var original=context.Response.Body;await using var buffer=new MemoryStream();context.Response.Body=buffer;
        try{await next(context);buffer.Position=0;var body=await new StreamReader(buffer).ReadToEndAsync(context.RequestAborted);if(context.Response.StatusCode is>=200 and<300){db.IdempotencyRecords.Add(new(){Scope=scope,Key=value,StatusCode=context.Response.StatusCode,ContentType=context.Response.ContentType,ResponseBody=body,CreatedAt=time.GetUtcNow(),ExpiresAt=time.GetUtcNow().AddHours(24)});await db.SaveChangesAsync(context.RequestAborted);}await transaction.CommitAsync(context.RequestAborted);buffer.Position=0;await buffer.CopyToAsync(original,context.RequestAborted);}finally{context.Response.Body=original;}
    }
}
