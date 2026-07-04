using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Application.Promises;

public sealed record CreatePromiseCommand(Guid CustomerId, decimal Amount, DateOnly PromiseDate, Guid CapturedByUserId);

public sealed class CreatePromiseHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
{
    public async Task<Guid> HandleAsync(CreatePromiseCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers
            .Include(c => c.Promises)
            .SingleOrDefaultAsync(c => c.Id == command.CustomerId, cancellationToken);

        if (customer is null) throw new InvalidOperationException("Customer was not found.");

        var promise = customer.CreatePromise(command.Amount, command.PromiseDate, clock.Today, command.CapturedByUserId);
        dbContext.AuditLog.Add(new AuditLog(
            "PromiseCreated",
            nameof(Customer),
            customer.Id,
            $"Promise {promise.Id} captured for {promise.Amount:C} on {promise.PromiseDate}.",
            command.CapturedByUserId));

        await dbContext.SaveChangesAsync(cancellationToken);
        return promise.Id;
    }
}
