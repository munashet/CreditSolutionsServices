using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Application.Payments;

public sealed record CapturePaymentCommand(Guid CustomerId, decimal Amount, DateOnly PaidOn, string Reference);

public sealed class CapturePaymentHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
{
    public async Task<Guid> HandleAsync(CapturePaymentCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers
            .Include(c => c.Promises)
            .Include(c => c.Payments)
            .SingleOrDefaultAsync(c => c.Id == command.CustomerId, cancellationToken);

        if (customer is null) throw new InvalidOperationException("Customer was not found.");

        var payment = customer.CapturePayment(command.Amount, command.PaidOn, command.Reference, clock.Today);
        dbContext.AuditLog.Add(new AuditLog(
            "PaymentCaptured",
            nameof(Customer),
            customer.Id,
            $"Payment {payment.Reference} for {payment.Amount:C} captured."));

        await dbContext.SaveChangesAsync(cancellationToken);
        return payment.Id;
    }
}
