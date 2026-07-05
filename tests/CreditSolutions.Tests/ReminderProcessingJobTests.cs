using CreditSolutions.Application.Abstractions;
using CreditSolutions.Application.Jobs;
using CreditSolutions.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CreditSolutions.Tests;

public sealed class ReminderProcessingJobTests
{
    [Fact]
    public async Task ProcessPendingRemindersAsync_creates_schema_before_querying_reminder_queue()
    {
        var databaseName = $"CreditSolutionsServices_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await EnsureDatabaseCanBeResetAsync(context);
        await context.Database.EnsureCreatedAsync();

        var sender = new TestReminderSender();
        var job = new ReminderProcessingJob(context, sender, NullLogger<ReminderProcessingJob>.Instance);

        await job.ProcessPendingRemindersAsync();

        var reminderCount = await context.ReminderQueue.CountAsync();
        Assert.Equal(0, reminderCount);
        Assert.Empty(sender.SentMessages);
    }

    private static async Task EnsureDatabaseCanBeResetAsync(ApplicationDbContext context)
    {
        try
        {
            await context.Database.EnsureDeletedAsync();
        }
        catch (SqlException ex) when (ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
        {
            // The database may not exist yet; the subsequent EnsureCreatedAsync call will create it.
        }
    }

    private sealed class TestReminderSender : IReminderSender
    {
        public List<string> SentMessages { get; } = new();

        public Task<string> SendSmsAsync(string mobileNumber, string message, CancellationToken cancellationToken)
        {
            SentMessages.Add($"{mobileNumber}:{message}");
            return Task.FromResult("sent");
        }
    }
}
