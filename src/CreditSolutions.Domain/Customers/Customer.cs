using CreditSolutions.Domain.Common;
using CreditSolutions.Domain.Payments;
using CreditSolutions.Domain.Promises;

namespace CreditSolutions.Domain.Customers;

public sealed class Customer : Entity
{
    private readonly List<Promise> _promises = [];
    private readonly List<Payment> _payments = [];

    private Customer() { }

    public Customer(string accountNumber, string fullName, string mobileNumber, decimal balance)
    {
        if (string.IsNullOrWhiteSpace(accountNumber)) throw new DomainException("Account number is required.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new DomainException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(mobileNumber)) throw new DomainException("Mobile number is required.");
        if (balance < 0) throw new DomainException("Balance cannot be negative.");

        AccountNumber = accountNumber.Trim();
        FullName = fullName.Trim();
        MobileNumber = mobileNumber.Trim();
        Balance = balance;
    }

    public string AccountNumber { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string MobileNumber { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public IReadOnlyCollection<Promise> Promises => _promises.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public Promise CreatePromise(decimal amount, DateOnly promiseDate, DateOnly today, Guid capturedByUserId)
    {
        if (amount <= 0) throw new DomainException("Promise amount must be greater than zero.");
        if (amount > Balance) throw new DomainException("Promise amount cannot exceed the customer balance.");
        if (promiseDate < today) throw new DomainException("Promise date cannot be in the past.");

        var promise = new Promise(Id, amount, promiseDate, capturedByUserId);
        _promises.Add(promise);
        return promise;
    }

    public Payment CapturePayment(decimal amount, DateOnly paidOn, string reference, DateOnly today)
    {
        if (amount <= 0) throw new DomainException("Payment amount must be greater than zero.");
        if (paidOn > today) throw new DomainException("Payment date cannot be in the future.");

        var payment = new Payment(Id, amount, paidOn, reference);
        Balance = Math.Max(0, Balance - amount);
        _payments.Add(payment);

        foreach (var promise in _promises.Where(p => p.Status == PromiseStatus.Pending))
        {
            promise.ApplyPayment(amount, paidOn);
        }

        return payment;
    }
}
