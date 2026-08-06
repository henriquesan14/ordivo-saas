using Carter;
using Ordivo.Api.Common;
using Ordivo.Application.Customers;
using Ordivo.Application.Customers.CreateCustomer;
using Ordivo.Application.Customers.GetCustomer;
using Ordivo.Application.Customers.ListCustomers;
using Ordivo.Application.Customers.UpdateCustomer;
using Ordivo.Application.Customers.ChangeCustomerStatus;
using Ordivo.Application.Common;
using Ordivo.SharedKernel.Messaging;
namespace Ordivo.Api.Endpoints;
public sealed class CustomerEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers").RequireAuthorization("TenantUser");
        group.MapPost("/", async (CreateCustomerCommand command, ICommandHandler<CreateCustomerCommand, CustomerDto> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.IsSuccess ? Results.Created($"/api/customers/{result.Value.Id}", result.Value) : result.ToHttpResult();
        });
        group.MapGet("/{id:guid}", async (Guid id, IQueryHandler<GetCustomerQuery, CustomerDto> handler, CancellationToken ct) =>
            (await handler.Handle(new GetCustomerQuery(id), ct)).ToHttpResult());
        group.MapGet("/", async (
            string? name,
            string? document,
            string? email,
            string? phone,
            bool? includeInactive,
            int? page,
            int? pageSize,
            string? sortBy,
            bool? descending,
            IQueryHandler<ListCustomersQuery, PagedResult<CustomerDto>> handler,
            CancellationToken ct) =>
            (await handler.Handle(new ListCustomersQuery(
                name, document, email, phone, includeInactive ?? false,
                page ?? 1,
                pageSize ?? 20,
                string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
                descending ?? false), ct)).ToHttpResult());

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCustomerRequest request,
            ICommandHandler<UpdateCustomerCommand, CustomerDto> handler,
            CancellationToken ct) =>
            (await handler.Handle(new UpdateCustomerCommand(
                id, request.Name, request.Document, request.Phone, request.Email), ct)).ToHttpResult());

        group.MapPatch("/{id:guid}/status", async (
            Guid id,
            ChangeCustomerStatusRequest request,
            ICommandHandler<ChangeCustomerStatusCommand, CustomerDto> handler,
            CancellationToken ct) =>
            (await handler.Handle(new ChangeCustomerStatusCommand(id, request.IsActive), ct)).ToHttpResult());
    }

    private sealed record UpdateCustomerRequest(string Name, string Document, string Phone, string? Email);
    private sealed record ChangeCustomerStatusRequest(bool IsActive);
}
