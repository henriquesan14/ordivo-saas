using Microsoft.EntityFrameworkCore;
using Ordivo.Api.Endpoints;
using Ordivo.Api.Authentication;
using Ordivo.Application;
using Ordivo.Infrastructure;
using Ordivo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdivoDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).WithTags("Health");
app.MapAuthenticationEndpoints();
app.MapTenantEndpoints();
app.MapCustomerEndpoints();
app.MapServiceOrderEndpoints();
app.Run();

public partial class Program;
