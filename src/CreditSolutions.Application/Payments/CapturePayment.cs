using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Customers;
using CreditSolutions.Domain.Payments;
using CreditSolutions.Domain.Promises;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Application.Payments;

public sealed record CapturePaymentCommand(Guid CustomerId, decimal Amount, DateOnly PaidOn, string Reference);

public sealed class CapturePaymentHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
{
    public async Task<Guid> HandleAsync(CapturePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers
            .SingleOrDefaultAsync(c => c.Id == command.CustomerId, cancellationToken);

        if (customer is null) throw new InvalidOperationException("Customer was not found.");

        // Business rule validation
        if (command.Amount <= 0) throw new InvalidOperationException("Payment amount must be greater than zero.");
        if (command.PaidOn > clock.Today) throw new InvalidOperationException("Payment date cannot be in the future.");
        if (string.IsNullOrWhiteSpace(command.Reference)) throw new InvalidOperationException("Payment reference is required.");

        var payment = new Payment(command.CustomerId, command.Amount, command.PaidOn, command.Reference);
        dbContext.Payments.Add(payment);

        // Update balance on the tracked customer entity (Balance has private setter)
        var customerEntry = ((DbContext)dbContext).Entry(customer);
        customerEntry.Property(c => c.Balance).CurrentValue =
            Math.Max(0, customer.Balance - command.Amount);
        customerEntry.Property(c => c.Balance).IsModified = true;

        // Apply payment to pending promises
        var pendingPromises = await dbContext.Promises
            .Where(p => p.CustomerId == command.CustomerId && p.Status == PromiseStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var promise in pendingPromises)
        {
            if (command.Amount >= promise.Amount && command.PaidOn <= promise.PromiseDate)
            {
                promise.ApplyPayment(command.Amount, command.PaidOn);
            }
        }

        dbContext.AuditLog.Add(new AuditLog(
            "PaymentCaptured",
            nameof(Customer),
            customer.Id,
            $"Payment {payment.Reference} for {payment.Amount:C} captured."));

        await dbContext.SaveChangesAsync(cancellationToken);
        return payment.Id;
    }
}
