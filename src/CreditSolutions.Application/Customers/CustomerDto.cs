namespace CreditSolutions.Application.Customers;

public sealed record CustomerDto(
    Guid Id,
    string AccountNumber,
    string FullName,
    string MobileNumber,
    decimal Balance,
    int PendingPromises,
    int BrokenPromises);
