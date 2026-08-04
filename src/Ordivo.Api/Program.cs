using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Carter;
using Ordivo.Api.Endpoints;
using Ordivo.Api.Authentication;
using Ordivo.Application;
using Ordivo.Infrastructure;
using Ordivo.Infrastructure.Persistence;
using Ordivo.Infrastructure.Authentication;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddCarter();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdivoDbContext>();
    await dbContext.Database.MigrateAsync();
}

await app.Services.SeedPlatformAdminAsync(app.Configuration);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).WithTags("Health");
app.MapCarter();
app.Run();

public partial class Program;
