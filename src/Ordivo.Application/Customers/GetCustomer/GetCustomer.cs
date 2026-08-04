using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;
namespace Ordivo.Application.Customers.GetCustomer;
public sealed record GetCustomerQuery(Guid CustomerId) : IQuery<CustomerDto>;
public sealed class GetCustomerQueryHandler(ICustomerRepository customers) : IQueryHandler<GetCustomerQuery, CustomerDto>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerQuery query, CancellationToken ct) =>
        await customers.GetAsync(query.CustomerId, ct) is { } customer
            ? Result.Success(customer.ToDto())
            : Result.Failure<CustomerDto>(Error.NotFound("Customer not found."));
}
