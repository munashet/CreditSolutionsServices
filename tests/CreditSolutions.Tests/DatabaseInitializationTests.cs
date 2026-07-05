using CreditSolutions.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CreditSolutions.Tests;

public sealed class DatabaseInitializationTests
{
    [Fact]
    public async Task EnsureDatabaseReadyAsync_creates_reminder_queue_table()
    {
        var databaseName = $"CreditSolutionsServices_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await EnsureDatabaseCanBeResetAsync(context);

        await DatabaseInitialization.EnsureDatabaseReadyAsync(context, NullLogger.Instance);

        await using var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ReminderQueue'";

        var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync());

        Assert.Equal(1, tableCount);
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
}
