namespace CreditSolutions.Application.Dashboard;

public sealed record DashboardSummary(
    int CustomerCount,
    int PendingPromises,
    int KeptPromises,
    int BrokenPromises,
    decimal PendingPromiseValue,
    int PendingReminders);
