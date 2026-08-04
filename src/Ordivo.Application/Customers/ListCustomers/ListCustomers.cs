using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;
namespace Ordivo.Application.Customers.ListCustomers;
public sealed record ListCustomersQuery : IQuery<IReadOnlyCollection<CustomerDto>>;
public sealed class ListCustomersQueryHandler(ICustomerRepository customers) : IQueryHandler<ListCustomersQuery, IReadOnlyCollection<CustomerDto>>
{
    public async Task<Result<IReadOnlyCollection<CustomerDto>>> Handle(ListCustomersQuery query, CancellationToken ct) =>
        Result.Success((await customers.ListAsync(ct)).ToListDto());
}
