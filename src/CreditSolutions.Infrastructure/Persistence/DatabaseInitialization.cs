using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CreditSolutions.Infrastructure.Persistence;

public static class DatabaseInitialization
{
    public static async Task EnsureDatabaseReadyAsync(ApplicationDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        logger.LogInformation("Database schema ensured successfully.");
    }
}
