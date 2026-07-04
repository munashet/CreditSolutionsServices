using CreditSolutions.Domain.Common;
using CreditSolutions.Domain.Customers;
using CreditSolutions.Domain.Promises;
using Xunit;

namespace CreditSolutions.Tests;

public sealed class PromiseDomainTests
{
    private static readonly Guid CollectorId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly DateOnly Today = new(2026, 7, 3);

    [Fact]
    public void CreatePromiseRejectsAmountGreaterThanBalance()
    {
        var customer = new Customer("ACC-1", "Test Customer", "+27820000000", 100m);

        var exception = Assert.Throws<DomainException>(() =>
            customer.CreatePromise(101m, Today.AddDays(1), Today, CollectorId));

        Assert.Contains("cannot exceed", exception.Message);
    }

    [Fact]
    public void CreatePromiseRejectsDateInPast()
    {
        var customer = new Customer("ACC-1", "Test Customer", "+27820000000", 100m);

        var exception = Assert.Throws<DomainException>(() =>
            customer.CreatePromise(50m, Today.AddDays(-1), Today, CollectorId));

        Assert.Contains("past", exception.Message);
    }

    [Fact]
    public void PaymentFulfillingPromiseMarksItKept()
    {
        var customer = new Customer("ACC-1", "Test Customer", "+27820000000", 100m);
        var promise = customer.CreatePromise(50m, Today, Today, CollectorId);

        customer.CapturePayment(50m, Today, "PMT-1", Today);

        Assert.Equal(PromiseStatus.Kept, promise.Status);
    }

    [Fact]
    public void OverduePromiseCanBeMarkedBrokenByBackgroundFlow()
    {
        var customer = new Customer("ACC-1", "Test Customer", "+27820000000", 100m);
        var promise = customer.CreatePromise(50m, Today, Today, CollectorId);

        promise.MarkBrokenFromBackgroundJob(Today.AddDays(1));

        Assert.Equal(PromiseStatus.Broken, promise.Status);
    }

    [Fact]
    public void PromiseDueTodayIsNotOverdue()
    {
        var customer = new Customer("ACC-1", "Test Customer", "+27820000000", 100m);
        var promise = customer.CreatePromise(50m, Today, Today, CollectorId);

        promise.MarkBrokenFromBackgroundJob(Today);

        Assert.Equal(PromiseStatus.Pending, promise.Status);
    }
}
