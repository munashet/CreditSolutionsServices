using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Customers;
using CreditSolutions.Domain.Payments;
using CreditSolutions.Domain.Promises;
using CreditSolutions.Domain.Reminders;
using CreditSolutions.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<User> Users { get; }
    DbSet<Promise> Promises { get; }
    DbSet<Payment> Payments { get; }
    DbSet<ReminderQueueItem> ReminderQueue { get; }
    DbSet<AuditLog> AuditLog { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
