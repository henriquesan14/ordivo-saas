using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Ordivo.Api.Common;

namespace Ordivo.Tests;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task Unexpected_exception_returns_safe_problem_details_in_production()
    {
        var context = CreateHttpContext();
        var handler = CreateHandler(Environments.Production);

        await handler.TryHandleAsync(context, new InvalidOperationException("sensitive detail"), default);

        var problem = await ReadProblemAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("Internal server error", problem.GetProperty("title").GetString());
        Assert.Equal("An unexpected error occurred.", problem.GetProperty("detail").GetString());
        Assert.Equal(context.TraceIdentifier, problem.GetProperty("traceId").GetString());
        Assert.DoesNotContain("sensitive detail", problem.ToString());
    }

    [Fact]
    public async Task Concurrency_exception_returns_conflict_problem_details()
    {
        var context = CreateHttpContext();
        var handler = CreateHandler(Environments.Production);

        await handler.TryHandleAsync(context, new DbUpdateConcurrencyException(), default);

        var problem = await ReadProblemAsync(context);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("Concurrency conflict", problem.GetProperty("title").GetString());
        Assert.Equal(context.TraceIdentifier, problem.GetProperty("traceId").GetString());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static GlobalExceptionHandler CreateHandler(string environmentName) =>
        new(NullLogger<GlobalExceptionHandler>.Instance, new TestHostEnvironment(environmentName));

    private static async Task<JsonElement> ReadProblemAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        return document.RootElement.Clone();
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Ordivo.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
