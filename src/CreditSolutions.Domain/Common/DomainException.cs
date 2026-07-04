namespace CreditSolutions.Domain.Common;

public sealed class DomainException(string message) : Exception(message);
