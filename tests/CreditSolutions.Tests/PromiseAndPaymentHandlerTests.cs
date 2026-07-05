using CreditSolutions.Application.Abstractions;
using CreditSolutions.Application.Customers;
using CreditSolutions.Application.Payments;
using CreditSolutions.Application.Promises;
using CreditSolutions.Domain.Promises;
using CreditSolutions.Infrastructure.Persistence;
using CreditSolutions.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CreditSolutions.Tests;

public sealed class PromiseAndPaymentHandlerTests : IAsyncLifetime
{
    private readonly string _databaseName = $"CreditSolutionsServices_Test_{Guid.NewGuid():N}";
    private ServiceProvider _services = null!;
    private IServiceScopeFactory _scopeFactory = null!;

    private static readonly Guid CollectorUserId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid CustomerOneId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly DateOnly Today = new(2026, 7, 5);

    public async Task InitializeAsync()
    {
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(Today));
        services.AddScoped<CreatePromiseHandler>();
        services.AddScoped<CapturePaymentHandler>();
        services.AddScoped<CreateCustomerHandler>();

        _services = services.BuildServiceProvider();
        _scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _services.DisposeAsync();

        // Clean up the test database
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var context = new ApplicationDbContext(options);
        try { await context.Database.EnsureDeletedAsync(); } catch { /* best effort */ }
    }

    [Fact]
    public async Task CreatePromise_succeeds_for_valid_customer()
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreatePromiseHandler>();

        var promiseId = await handler.HandleAsync(new CreatePromiseCommand(
            CustomerOneId, 200m, Today.AddDays(3), CollectorUserId));

        Assert.NotEqual(Guid.Empty, promiseId);
    }

    [Fact]
    public async Task CreatePromise_twice_on_same_scope_succeeds()
    {
        // This simulates the Blazor circuit-scoped DbContext being reused
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreatePromiseHandler>();

        await handler.HandleAsync(new CreatePromiseCommand(
            CustomerOneId, 100m, Today.AddDays(1), CollectorUserId));

        // Second call on same scope — should not throw concurrency error
        var promiseId = await handler.HandleAsync(new CreatePromiseCommand(
            CustomerOneId, 200m, Today.AddDays(2), CollectorUserId));

        Assert.NotEqual(Guid.Empty, promiseId);
    }

    [Fact]
    public async Task CreatePromise_twice_on_different_scopes_succeeds()
    {
        using var scope1 = _scopeFactory.CreateScope();
        var handler1 = scope1.ServiceProvider.GetRequiredService<CreatePromiseHandler>();
        await handler1.HandleAsync(new CreatePromiseCommand(
            CustomerOneId, 100m, Today.AddDays(1), CollectorUserId));

        using var scope2 = _scopeFactory.CreateScope();
        var handler2 = scope2.ServiceProvider.GetRequiredService<CreatePromiseHandler>();
        var promiseId = await handler2.HandleAsync(new CreatePromiseCommand(
            CustomerOneId, 200m, Today.AddDays(2), CollectorUserId));

        Assert.NotEqual(Guid.Empty, promiseId);
    }

    [Fact]
    public async Task CapturePayment_succeeds_for_valid_customer()
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CapturePaymentHandler>();

        var paymentId = await handler.HandleAsync(new CapturePaymentCommand(
            CustomerOneId, 50m, Today, "PMT-TEST-1"));

        Assert.NotEqual(Guid.Empty, paymentId);
    }

    [Fact]
    public async Task CapturePayment_twice_on_same_scope_succeeds()
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CapturePaymentHandler>();

        await handler.HandleAsync(new CapturePaymentCommand(
            CustomerOneId, 50m, Today, "PMT-A"));

        var paymentId = await handler.HandleAsync(new CapturePaymentCommand(
            CustomerOneId, 75m, Today, "PMT-B"));

        Assert.NotEqual(Guid.Empty, paymentId);
    }

    [Fact]
    public async Task CapturePayment_twice_on_different_scopes_succeeds()
    {
        using var scope1 = _scopeFactory.CreateScope();
        var handler1 = scope1.ServiceProvider.GetRequiredService<CapturePaymentHandler>();
        await handler1.HandleAsync(new CapturePaymentCommand(
            CustomerOneId, 50m, Today, "PMT-X"));

        using var scope2 = _scopeFactory.CreateScope();
        var handler2 = scope2.ServiceProvider.GetRequiredService<CapturePaymentHandler>();
        var paymentId = await handler2.HandleAsync(new CapturePaymentCommand(
            CustomerOneId, 75m, Today, "PMT-Y"));

        Assert.NotEqual(Guid.Empty, paymentId);
    }

    [Fact]
    public async Task CapturePayment_marks_promise_as_kept_when_fully_paid()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // First create a promise
        var promiseHandler = scope.ServiceProvider.GetRequiredService<CreatePromiseHandler>();
        var promiseId = await promiseHandler.HandleAsync(new CreatePromiseCommand(
            CustomerOneId, 80m, Today, CollectorUserId));

        // Then capture a payment that covers it
        var paymentHandler = scope.ServiceProvider.GetRequiredService<CapturePaymentHandler>();
        await paymentHandler.HandleAsync(new CapturePaymentCommand(
            CustomerOneId, 80m, Today, "PMT-FULL"));

        // Verify promise is Kept
        var promise = await db.Promises.SingleAsync(p => p.Id == promiseId);
        Assert.Equal(PromiseStatus.Kept, promise.Status);
    }

    [Fact]
    public async Task CreatePromise_rejects_amount_greater_than_balance()
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreatePromiseHandler>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new CreatePromiseCommand(
                CustomerOneId, 9999m, Today.AddDays(1), CollectorUserId)));

        Assert.Contains("cannot exceed", exception.Message);
    }

    [Fact]
    public async Task CreatePromise_rejects_past_date()
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreatePromiseHandler>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new CreatePromiseCommand(
                CustomerOneId, 100m, Today.AddDays(-1), CollectorUserId)));

        Assert.Contains("past", exception.Message);
    }

    [Fact]
    public async Task CreatePromise_persists_to_database()
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreatePromiseHandler>();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var promiseId = await handler.HandleAsync(new CreatePromiseCommand(
            CustomerOneId, 150m, Today.AddDays(5), CollectorUserId));

        var promise = await db.Promises.SingleAsync(p => p.Id == promiseId);
        Assert.Equal(150m, promise.Amount);
        Assert.Equal(Today.AddDays(5), promise.PromiseDate);
        Assert.Equal(PromiseStatus.Pending, promise.Status);
    }

    [Fact]
    public async Task CapturePayment_persists_to_database()
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CapturePaymentHandler>();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var paymentId = await handler.HandleAsync(new CapturePaymentCommand(
            CustomerOneId, 30m, Today, "PMT-PERSIST"));

        var payment = await db.Payments.SingleAsync(p => p.Id == paymentId);
        Assert.Equal(30m, payment.Amount);
        Assert.Equal("PMT-PERSIST", payment.Reference);
    }

    private sealed class FixedDateTimeProvider(DateOnly today) : IDateTimeProvider
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => new(today, TimeOnly.MinValue, TimeSpan.Zero);
    }
}
