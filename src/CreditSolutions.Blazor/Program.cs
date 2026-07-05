using CreditSolutions.Application;
using CreditSolutions.Blazor.Components;
using CreditSolutions.Infrastructure;
using CreditSolutions.Infrastructure.Hangfire;
using CreditSolutions.Infrastructure.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        await DatabaseInitialization.EnsureDatabaseReadyAsync(dbContext, app.Logger);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database initialization failed. The application will continue, but data access may be unavailable.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

var hasHangfireServices = builder.Services.Any(service => service.ServiceType == typeof(JobStorage));

if (hasHangfireServices)
{
    app.UseHangfireDashboard("/jobs");
    HangfireJobScheduler.ScheduleRecurringJobs();
}
else
{
    app.Logger.LogWarning("Hangfire dashboard and recurring jobs are disabled because the background services were not registered.");
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
