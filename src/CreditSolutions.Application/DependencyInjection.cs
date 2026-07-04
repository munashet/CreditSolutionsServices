using CreditSolutions.Application.Customers;
using CreditSolutions.Application.Dashboard;
using CreditSolutions.Application.Jobs;
using CreditSolutions.Application.Payments;
using CreditSolutions.Application.Promises;
using Microsoft.Extensions.DependencyInjection;

namespace CreditSolutions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateCustomerHandler>();
        services.AddScoped<SearchCustomersHandler>();
        services.AddScoped<CreatePromiseHandler>();
        services.AddScoped<GetPromisesHandler>();
        services.AddScoped<CapturePaymentHandler>();
        services.AddScoped<GetDashboardSummaryHandler>();
        services.AddScoped<PromiseMonitoringJob>();
        services.AddScoped<ReminderProcessingJob>();
        return services;
    }
}
