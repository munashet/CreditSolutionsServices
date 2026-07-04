using CreditSolutions.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CreditSolutions.Infrastructure.Services;

public sealed class SimulatedSmsReminderSender(ILogger<SimulatedSmsReminderSender> logger) : IReminderSender
{
    public Task<string> SendSmsAsync(string mobileNumber, string message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Simulated SMS to {MobileNumber}: {Message}", mobileNumber, message);
        return Task.FromResult($"Simulated SMS sent to {mobileNumber} at {DateTimeOffset.UtcNow:u}.");
    }
}
