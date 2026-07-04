using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Customers;
using CreditSolutions.Domain.Promises;
using CreditSolutions.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Infrastructure.Persistence;

public static class SeedData
{
    public static readonly Guid CollectorUserId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid CustomerOneId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid CustomerTwoId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid PendingPromiseId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid BrokenCandidatePromiseId = Guid.Parse("30000000-0000-0000-0000-000000000002");

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            CreateUser(CollectorUserId, "Demo Collector", "collector@css.local"));

        modelBuilder.Entity<Customer>().HasData(
            CreateCustomer(CustomerOneId, "ACC-1001", "Nandi Dlamini", "+27821234567", 1500m),
            CreateCustomer(CustomerTwoId, "ACC-1002", "Johan Botha", "+27829876543", 950m));

        modelBuilder.Entity<Promise>().HasData(
            CreatePromise(PendingPromiseId, CustomerOneId, 500m, new DateOnly(2026, 7, 5), PromiseStatus.Pending),
            CreatePromise(BrokenCandidatePromiseId, CustomerTwoId, 300m, new DateOnly(2026, 7, 2), PromiseStatus.Pending));

        modelBuilder.Entity<AuditLog>().HasData(
            CreateAudit(Guid.Parse("40000000-0000-0000-0000-000000000001"), "SeedData", "System", CollectorUserId, "Demo data seeded."));
    }

    private static User CreateUser(Guid id, string displayName, string email)
    {
        var user = new User(displayName, email);
        SetId(user, id);
        return user;
    }

    private static Customer CreateCustomer(Guid id, string accountNumber, string fullName, string mobile, decimal balance)
    {
        var customer = new Customer(accountNumber, fullName, mobile, balance);
        SetId(customer, id);
        return customer;
    }

    private static Promise CreatePromise(Guid id, Guid customerId, decimal amount, DateOnly date, PromiseStatus status)
    {
        var promise = new Promise(customerId, amount, date, CollectorUserId);
        SetId(promise, id);
        typeof(Promise).GetProperty(nameof(Promise.Status))!.SetValue(promise, status);
        return promise;
    }

    private static AuditLog CreateAudit(Guid id, string action, string entityName, Guid entityId, string details)
    {
        var audit = new AuditLog(action, entityName, entityId, details);
        SetId(audit, id);
        return audit;
    }

    private static void SetId(object entity, Guid id)
    {
        entity.GetType().BaseType!.GetProperty("Id")!.SetValue(entity, id);
    }
}
