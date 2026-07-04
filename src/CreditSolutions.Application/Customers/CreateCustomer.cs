using CreditSolutions.Application.Abstractions;
using CreditSolutions.Domain.Auditing;
using CreditSolutions.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace CreditSolutions.Application.Customers;

public sealed record CreateCustomerCommand(string AccountNumber, string FullName, string MobileNumber, decimal Balance);

public sealed class CreateCustomerHandler(IApplicationDbContext dbContext)
{
    public async Task<Guid> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Customers.AnyAsync(c => c.AccountNumber == command.AccountNumber, cancellationToken);
        if (exists) throw new InvalidOperationException("A customer with this account number already exists.");

        var customer = new Customer(command.AccountNumber, command.FullName, command.MobileNumber, command.Balance);
        dbContext.Customers.Add(customer);
        dbContext.AuditLog.Add(new AuditLog("CustomerCreated", nameof(Customer), customer.Id, $"Customer {customer.AccountNumber} created."));
        await dbContext.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }
}
