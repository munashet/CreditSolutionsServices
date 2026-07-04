using CreditSolutions.Application.Abstractions;

namespace CreditSolutions.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.Date);
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
