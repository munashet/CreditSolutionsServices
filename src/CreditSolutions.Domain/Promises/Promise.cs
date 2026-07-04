using CreditSolutions.Domain.Common;

namespace CreditSolutions.Domain.Promises;

public sealed class Promise : Entity
{
    private Promise() { }

    public Promise(Guid customerId, decimal amount, DateOnly promiseDate, Guid capturedByUserId)
    {
        CustomerId = customerId;
        Amount = amount;
        PromiseDate = promiseDate;
        CapturedByUserId = capturedByUserId;
        Status = PromiseStatus.Pending;
    }

    public Guid CustomerId { get; private set; }
    public Guid CapturedByUserId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PromiseDate { get; private set; }
    public PromiseStatus Status { get; private set; }
    public DateTimeOffset? KeptAt { get; private set; }
    public DateTimeOffset? BrokenAt { get; private set; }

    public bool IsOverdue(DateOnly today) => Status == PromiseStatus.Pending && PromiseDate < today;

    public void ApplyPayment(decimal paymentAmount, DateOnly paidOn)
    {
        if (Status != PromiseStatus.Pending) return;
        if (paymentAmount >= Amount && paidOn <= PromiseDate)
        {
            Status = PromiseStatus.Kept;
            KeptAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkBrokenFromBackgroundJob(DateOnly today)
    {
        if (!IsOverdue(today)) return;

        Status = PromiseStatus.Broken;
        BrokenAt = DateTimeOffset.UtcNow;
    }
}
