using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Reminders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CreditSolutions.Application.Jobs;

public sealed class ReminderProcessingJob(
    IApplicationDbContext dbContext,
    IReminderSender reminderSender,
    ILogger<ReminderProcessingJob> logger)
{
    public async Task ProcessPendingRemindersAsync(CancellationToken cancellationToken = default)
    {
        var reminders = await dbContext.ReminderQueue
            .Where(r => r.Status == ReminderStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var reminder in reminders)
        {
            var outcome = await reminderSender.SendSmsAsync(reminder.MobileNumber, reminder.Message, cancellationToken);
            reminder.MarkProcessed(outcome);

            dbContext.AuditLog.Add(new AuditLog(
                "ReminderProcessed",
                "ReminderQueue",
                reminder.Id,
                outcome));

            logger.LogInformation("Processed reminder {ReminderId}: {Outcome}", reminder.Id, outcome);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
