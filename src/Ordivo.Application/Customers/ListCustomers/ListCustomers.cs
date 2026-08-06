using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Application.Common;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Customers.ListCustomers;

public sealed record ListCustomersQuery(
    string? Name = null,
    string? Document = null,
    string? Email = null,
    string? Phone = null,
    bool IncludeInactive = false,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "name",
    bool Descending = false) : IQuery<PagedResult<CustomerDto>>;

public sealed class ListCustomersQueryHandler(ICustomerRepository customers)
    : IQueryHandler<ListCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<Result<PagedResult<CustomerDto>>> Handle(ListCustomersQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var result = await customers.ListAsync(
            query.Name, query.Document, query.Email, query.Phone, query.IncludeInactive,
            page, pageSize, query.SortBy, query.Descending, ct);
        return Result.Success(new PagedResult<CustomerDto>(
            result.Items.ToListDto(), page, pageSize, result.TotalCount));
    }
}
