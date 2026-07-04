using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Customers;
using CreditSolutions.Domain.Payments;
using CreditSolutions.Domain.Promises;
using CreditSolutions.Domain.Reminders;
using CreditSolutions.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Promise> Promises => Set<Promise>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ReminderQueueItem> ReminderQueue => Set<ReminderQueueItem>();
    public DbSet<AuditLog> AuditLog => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        SeedData.Apply(modelBuilder);
    }
}
