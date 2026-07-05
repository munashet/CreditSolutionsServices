using System.Linq.Expressions;
using CreditSolutions.Infrastructure;
using CreditSolutions.Infrastructure.Hangfire;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CreditSolutions.Tests;

public sealed class HangfireJobSchedulerTests
{
    [Fact]
    public void ScheduleRecurringJobs_DoesNotThrow_WhenSchedulerFails()
    {
        var scheduler = new ThrowingRecurringJobScheduler();
        var logger = new TestLogger();

        var exception = Record.Exception(() => HangfireJobScheduler.ScheduleRecurringJobs(scheduler, logger));

        Assert.Null(exception);
        Assert.Contains(logger.Messages, message => message.Contains("could not be scheduled", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CanConnectToDatabase_ReturnsTrue_ForValidLocalSqliteFile()
    {
        var result = DependencyInjection.CanConnectToDatabase("Server=.\\SQLEXPRESS;Database=CreditSolutionsServicesTest;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3");

        Assert.True(result);
    }

    private sealed class ThrowingRecurringJobScheduler : IRecurringJobScheduler
    {
        public void AddOrUpdate<T>(string recurringJobId, Expression<Action<T>> methodCall, Func<string> cronExpression)
            => throw new InvalidOperationException("database unavailable");
    }

    private sealed class TestLogger : ILogger
    {
        public List<string> Messages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
