using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Customers;
using CreditSolutions.Domain.Promises;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Application.Promises;

public sealed record CreatePromiseCommand(Guid CustomerId, decimal Amount, DateOnly PromiseDate, Guid CapturedByUserId);

public sealed class CreatePromiseHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
{
    public async Task<Guid> HandleAsync(CreatePromiseCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers
            .SingleOrDefaultAsync(c => c.Id == command.CustomerId, cancellationToken);

        if (customer is null) throw new InvalidOperationException("Customer was not found.");

        // Business rule validation (duplicated from Customer.CreatePromise)
        if (command.Amount <= 0) throw new InvalidOperationException("Promise amount must be greater than zero.");
        if (command.Amount > customer.Balance) throw new InvalidOperationException("Promise amount cannot exceed the customer balance.");
        if (command.PromiseDate < clock.Today) throw new InvalidOperationException("Promise date cannot be in the past.");

        var promise = new Promise(command.CustomerId, command.Amount, command.PromiseDate, command.CapturedByUserId);
        dbContext.Promises.Add(promise);

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
