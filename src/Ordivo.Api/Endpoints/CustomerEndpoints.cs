using Ordivo.Api.Common;
using Ordivo.Application.Customers;
using Ordivo.Application.Customers.CreateCustomer;
using Ordivo.Application.Customers.GetCustomer;
using Ordivo.Application.Customers.ListCustomers;
using Ordivo.SharedKernel.Messaging;
namespace Ordivo.Api.Endpoints;
public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers").RequireAuthorization();
        group.MapPost("/", async (CreateCustomerCommand command, ICommandHandler<CreateCustomerCommand, CustomerDto> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.IsSuccess ? Results.Created($"/api/customers/{result.Value.Id}", result.Value) : result.ToHttpResult();
        });
        group.MapGet("/{id:guid}", async (Guid id, IQueryHandler<GetCustomerQuery, CustomerDto> handler, CancellationToken ct) =>
            (await handler.Handle(new GetCustomerQuery(id), ct)).ToHttpResult());
        group.MapGet("/", async (IQueryHandler<ListCustomersQuery, IReadOnlyCollection<CustomerDto>> handler, CancellationToken ct) =>
            (await handler.Handle(new ListCustomersQuery(), ct)).ToHttpResult());
        return app;
    }
}
