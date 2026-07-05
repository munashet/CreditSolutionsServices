using System.Linq.Expressions;
using CreditSolutions.Application.Jobs;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CreditSolutions.Infrastructure.Hangfire;

public static class HangfireJobScheduler
{
    public static void ScheduleRecurringJobs(IRecurringJobScheduler? scheduler = null, ILogger? logger = null)
    {
        var jobScheduler = scheduler ?? new HangfireRecurringJobScheduler();
        var log = logger ?? NullLogger.Instance;

        try
        {
            jobScheduler.AddOrUpdate<PromiseMonitoringJob>(
                "ptp-hourly-overdue-monitor",
                job => job.MarkOverduePromisesBrokenAsync(CancellationToken.None),
                () => Cron.Hourly());

            jobScheduler.AddOrUpdate<ReminderProcessingJob>(
                "ptp-minute-reminder-processor",
                job => job.ProcessPendingRemindersAsync(CancellationToken.None),
                () => Cron.Minutely());
        }
        catch (Exception ex)
        {
            log.LogWarning(ex,
                "Hangfire recurring jobs could not be scheduled because the background storage is unavailable. The application will continue to run.");
        }
    }
}

public interface IRecurringJobScheduler
{
    void AddOrUpdate<T>(string recurringJobId, Expression<Action<T>> methodCall, Func<string> cronExpression);
}

public sealed class HangfireRecurringJobScheduler : IRecurringJobScheduler
{
    public void AddOrUpdate<T>(string recurringJobId, Expression<Action<T>> methodCall, Func<string> cronExpression)
        => RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression);
}
