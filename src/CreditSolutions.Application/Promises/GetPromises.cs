using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Promises;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Application.Promises;

public sealed record PromiseListQuery(PromiseStatus? Status, DateOnly? From, DateOnly? To);

public sealed class GetPromisesHandler(IApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<PromiseDto>> HandleAsync(PromiseListQuery query, CancellationToken cancellationToken = default)
    {
        var promises = dbContext.Promises
            .Join(dbContext.Customers,
                p => p.CustomerId,
                c => c.Id,
                (p, c) => new { Promise = p, Customer = c })
            .AsNoTracking();

        if (query.Status is not null) promises = promises.Where(x => x.Promise.Status == query.Status);
        if (query.From is not null) promises = promises.Where(x => x.Promise.PromiseDate >= query.From);
        if (query.To is not null) promises = promises.Where(x => x.Promise.PromiseDate <= query.To);

        return await promises
            .OrderBy(x => x.Promise.PromiseDate)
            .Select(x => new PromiseDto(
                x.Promise.Id,
                x.Promise.CustomerId,
                x.Customer.FullName,
                x.Promise.Amount,
                x.Promise.PromiseDate,
                x.Promise.Status))
            .ToListAsync(cancellationToken);
    }
}
