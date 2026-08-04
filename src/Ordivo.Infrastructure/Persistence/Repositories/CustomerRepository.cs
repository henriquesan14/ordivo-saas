using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Customers;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(OrdivoDbContext dbContext) : ICustomerRepository
{
    public async Task AddAsync(Customer customer, CancellationToken ct) => await dbContext.Customers.AddAsync(customer, ct);
    public Task<Customer?> GetAsync(Guid id, CancellationToken ct) => dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, ct);
    public Task<bool> DocumentExistsAsync(string document, CancellationToken ct) => dbContext.Customers.AnyAsync(customer => customer.Document == document, ct);
    public async Task<IReadOnlyCollection<Customer>> ListAsync(CancellationToken ct) =>
        await dbContext.Customers.AsNoTracking().OrderBy(customer => customer.Name).ToListAsync(ct);
}
