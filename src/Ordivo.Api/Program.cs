using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json.Serialization;
using Carter;
using Ordivo.Api.Endpoints;
using Ordivo.Api.Authentication;
using Ordivo.Api.Common;
using Ordivo.Api.Security;
using Ordivo.Application;
using Ordivo.Infrastructure;
using Ordivo.Infrastructure.Persistence;
using Ordivo.Infrastructure.Authentication;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Net;
using Ordivo.Infrastructure.Payments;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
        if (IPAddress.TryParse(proxy, out var address)) options.KnownProxies.Add(address);
});
var telemetry = builder.Services.AddOpenTelemetry().ConfigureResource(resource => resource.AddService("Ordivo.Api"));
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
telemetry.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddSource("Npgsql");
    if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var endpoint)) tracing.AddOtlpExporter(options => options.Endpoint = endpoint);
});
telemetry.WithMetrics(metrics =>
{
    metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation();
    if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var endpoint)) metrics.AddOtlpExporter(options => options.Endpoint = endpoint);
});
var dataProtectionPath = builder.Configuration.GetValue("DataProtection:KeysPath", ".keys");
Directory.CreateDirectory(dataProtectionPath);
builder.Services.AddDataProtection()
    .SetApplicationName("Ordivo")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddCarter();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApiSecurity(builder.Configuration, builder.Environment);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseForwardedHeaders();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdivoDbContext>();
    await dbContext.Database.MigrateAsync();
}

await app.Services.SeedDefaultPlanAsync(app.Configuration);
await app.Services.SeedPlatformAdminAsync(app.Configuration);

app.UseHttpsRedirection();
app.UseCors(SecurityExtensions.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ImpersonationProtectionMiddleware>();
app.UseRateLimiter();
app.UseApiCsrfProtection();
app.UseMiddleware<IdempotencyMiddleware>();
app.UseMiddleware<CommercialAccessMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/health");
app.MapCarter();
app.Run();

public partial class Program;
