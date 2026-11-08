namespace MotiWriter.Core.Dashboard;

public interface IDashboardService
{
    /// <param name="weeks">Nombre de semaines du calendrier (lundi → dimanche).</param>
    /// <param name="recentDays">Nombre de jours de la bande des derniers jours, aujourd'hui compris.</param>
    Task<DashboardSummary> GetSummaryAsync(int weeks = 20, int recentDays = 8, CancellationToken cancellationToken = default);
}
