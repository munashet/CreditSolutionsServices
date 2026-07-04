namespace CreditSolutions.Application.Abstractions;

public interface IReminderSender
{
    Task<string> SendSmsAsync(string mobileNumber, string message, CancellationToken cancellationToken);
}
