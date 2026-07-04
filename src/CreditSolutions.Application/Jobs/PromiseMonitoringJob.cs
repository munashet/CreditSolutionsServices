using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Reminders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CreditSolutions.Application.Jobs;

public sealed class PromiseMonitoringJob(
    IApplicationDbContext dbContext,
    IDateTimeProvider clock,
    ILogger<PromiseMonitoringJob> logger)
{
    public async Task MarkOverduePromisesBrokenAsync(CancellationToken cancellationToken = default)
    {
        var overduePromises = await dbContext.Promises
            .Where(p => p.PromiseDate < clock.Today)
            .ToListAsync(cancellationToken);

        foreach (var promise in overduePromises.Where(p => p.IsOverdue(clock.Today)))
        {
            var customer = await dbContext.Customers.SingleAsync(c => c.Id == promise.CustomerId, cancellationToken);
            promise.MarkBrokenFromBackgroundJob(clock.Today);

            dbContext.AuditLog.Add(new AuditLog(
                "PromiseBroken",
                "Promise",
                promise.Id,
                $"Promise due {promise.PromiseDate} was marked broken by the hourly job."));

            dbContext.ReminderQueue.Add(new ReminderQueueItem(
                customer.Id,
                promise.Id,
                customer.MobileNumber,
                $"Your promise to pay {promise.Amount:C} was missed. Please contact CSS Credit Solutions."));

            logger.LogInformation("Marked promise {PromiseId} as broken and queued reminder.", promise.Id);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
