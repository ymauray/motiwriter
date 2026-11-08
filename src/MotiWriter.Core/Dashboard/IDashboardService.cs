namespace MotiWriter.Core.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync(int weeks = 20, CancellationToken cancellationToken = default);
}
