using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Promises;
using CreditSolutions.Domain.Reminders;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Application.Dashboard;

public sealed class GetDashboardSummaryHandler(IApplicationDbContext dbContext)
{
    public async Task<DashboardSummary> HandleAsync(CancellationToken cancellationToken = default)
    {
        return new DashboardSummary(
            await dbContext.Customers.CountAsync(cancellationToken),
            await dbContext.Promises.CountAsync(p => p.Status == PromiseStatus.Pending, cancellationToken),
            await dbContext.Promises.CountAsync(p => p.Status == PromiseStatus.Kept, cancellationToken),
            await dbContext.Promises.CountAsync(p => p.Status == PromiseStatus.Broken, cancellationToken),
            await dbContext.Promises.Where(p => p.Status == PromiseStatus.Pending).SumAsync(p => p.Amount, cancellationToken),
            await dbContext.ReminderQueue.CountAsync(r => r.Status == ReminderStatus.Pending, cancellationToken));
    }
}
