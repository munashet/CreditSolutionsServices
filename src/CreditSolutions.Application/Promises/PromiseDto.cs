using CreditSolutions.Domain.Promises;

namespace CreditSolutions.Application.Promises;

public sealed record PromiseDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string AccountNumber,
    decimal Amount,
    DateOnly PromiseDate,
    PromiseStatus Status);
