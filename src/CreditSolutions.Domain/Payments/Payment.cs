using CreditSolutions.Domain.Common;

namespace CreditSolutions.Domain.Payments;

public sealed class Payment : Entity
{
    private Payment() { }

    public Payment(Guid customerId, decimal amount, DateOnly paidOn, string reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) throw new DomainException("Payment reference is required.");

        CustomerId = customerId;
        Amount = amount;
        PaidOn = paidOn;
        Reference = reference.Trim();
    }

    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaidOn { get; private set; }
    public string Reference { get; private set; } = string.Empty;
}
