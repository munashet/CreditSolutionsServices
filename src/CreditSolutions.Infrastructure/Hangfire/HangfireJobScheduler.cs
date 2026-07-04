using CreditSolutions.Application.Jobs;
using Hangfire;

namespace CreditSolutions.Infrastructure.Hangfire;

public static class HangfireJobScheduler
{
    public static void ScheduleRecurringJobs()
    {
        RecurringJob.AddOrUpdate<PromiseMonitoringJob>(
            "ptp-hourly-overdue-monitor",
            job => job.MarkOverduePromisesBrokenAsync(CancellationToken.None),
            Cron.Hourly);

        RecurringJob.AddOrUpdate<ReminderProcessingJob>(
            "ptp-minute-reminder-processor",
            job => job.ProcessPendingRemindersAsync(CancellationToken.None),
            Cron.Minutely);
    }
}
