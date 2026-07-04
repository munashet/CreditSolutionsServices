using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Promises;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Application.Customers;

public sealed record CustomerSearchQuery(string? SearchTerm, PromiseStatus? PromiseStatus);

public sealed class SearchCustomersHandler(IApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<CustomerDto>> HandleAsync(CustomerSearchQuery query, CancellationToken cancellationToken = default)
    {
        var customers = dbContext.Customers
            .Include(c => c.Promises)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            customers = customers.Where(c =>
                c.AccountNumber.Contains(term) ||
                c.FullName.Contains(term) ||
                c.MobileNumber.Contains(term));
        }

        if (query.PromiseStatus is not null)
        {
            customers = customers.Where(c => c.Promises.Any(p => p.Status == query.PromiseStatus));
        }

        return await customers
            .OrderBy(c => c.FullName)
            .Select(c => new CustomerDto(
                c.Id,
                c.AccountNumber,
                c.FullName,
                c.MobileNumber,
                c.Balance,
                c.Promises.Count(p => p.Status == PromiseStatus.Pending),
                c.Promises.Count(p => p.Status == PromiseStatus.Broken)))
            .ToListAsync(cancellationToken);
    }
}
