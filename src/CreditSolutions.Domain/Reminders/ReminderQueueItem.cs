using CreditSolutions.Domain.Common;

namespace CreditSolutions.Domain.Reminders;

public sealed class ReminderQueueItem : Entity
{
    private ReminderQueueItem() { }

    public ReminderQueueItem(Guid customerId, Guid promiseId, string mobileNumber, string message)
    {
        CustomerId = customerId;
        PromiseId = promiseId;
        MobileNumber = mobileNumber;
        Message = message;
        Status = ReminderStatus.Pending;
    }

    public Guid CustomerId { get; private set; }
    public Guid PromiseId { get; private set; }
    public string MobileNumber { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public ReminderStatus Status { get; private set; }
    public string? Outcome { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    public void MarkProcessed(string outcome)
    {
        Status = ReminderStatus.Processed;
        Outcome = outcome;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}
