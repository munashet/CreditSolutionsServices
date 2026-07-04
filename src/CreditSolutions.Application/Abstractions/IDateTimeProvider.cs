namespace CreditSolutions.Application.Abstractions;

public interface IDateTimeProvider
{
    DateOnly Today { get; }
    DateTimeOffset UtcNow { get; }
}
