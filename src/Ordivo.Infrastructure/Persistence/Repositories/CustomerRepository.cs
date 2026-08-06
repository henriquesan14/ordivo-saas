using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Customers;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(OrdivoDbContext dbContext) : ICustomerRepository
{
    public async Task AddAsync(Customer customer, CancellationToken ct) => await dbContext.Customers.AddAsync(customer, ct);
    public Task<Customer?> GetAsync(Guid id, CancellationToken ct) => dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, ct);
    public Task<bool> DocumentExistsAsync(string document, CancellationToken ct, Guid? excludingCustomerId = null) =>
        dbContext.Customers.AnyAsync(customer =>
            customer.Document == document && (!excludingCustomerId.HasValue || customer.Id != excludingCustomerId.Value), ct);

    public async Task<(IReadOnlyCollection<Customer> Items, int TotalCount)> ListAsync(
        string? name,
        string? document,
        string? email,
        string? phone,
        bool includeInactive,
        int page,
        int pageSize,
        string sortBy,
        bool descending,
        CancellationToken ct)
    {
        var query = dbContext.Customers.AsNoTracking();
        if (!includeInactive) query = query.Where(customer => customer.IsActive);
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(customer => EF.Functions.ILike(customer.Name, $"%{name.Trim()}%"));
        if (!string.IsNullOrWhiteSpace(document))
            query = query.Where(customer => EF.Functions.ILike(customer.Document, $"%{document.Trim()}%"));
        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(customer => customer.Email != null && EF.Functions.ILike(customer.Email, $"%{email.Trim()}%"));
        if (!string.IsNullOrWhiteSpace(phone))
            query = query.Where(customer => EF.Functions.ILike(customer.Phone, $"%{phone.Trim()}%"));

        var totalCount = await query.CountAsync(ct);
        query = (sortBy.Trim().ToLowerInvariant(), descending) switch
        {
            ("document", false) => query.OrderBy(customer => customer.Document),
            ("document", true) => query.OrderByDescending(customer => customer.Document),
            ("email", false) => query.OrderBy(customer => customer.Email),
            ("email", true) => query.OrderByDescending(customer => customer.Email),
            ("phone", false) => query.OrderBy(customer => customer.Phone),
            ("phone", true) => query.OrderByDescending(customer => customer.Phone),
            ("createdat", false) => query.OrderBy(customer => customer.CreatedAt),
            ("createdat", true) => query.OrderByDescending(customer => customer.CreatedAt),
            ("updatedat", false) => query.OrderBy(customer => customer.UpdatedAt),
            ("updatedat", true) => query.OrderByDescending(customer => customer.UpdatedAt),
            ("name", true) => query.OrderByDescending(customer => customer.Name),
            _ => query.OrderBy(customer => customer.Name)
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, totalCount);
    }
}
