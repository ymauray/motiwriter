using MotiWriter.Core.Statistics;

namespace MotiWriter.Core.Dashboard;

/// <summary>Tout ce dont la page d'accueil a besoin, en un seul objet sérialisable.</summary>
/// <param name="Today">Date de référence du calcul.</param>
/// <param name="WrittenToday">Vrai si aujourd'hui est déjà marqué écrit.</param>
/// <param name="Statistics">Statistiques agrégées.</param>
/// <param name="Weeks">Calendrier des dernières semaines (lundi → dimanche), la dernière étant la semaine en cours.</param>
/// <param name="RecentDays">Les derniers jours (huit par défaut), du plus ancien à aujourd'hui inclus.</param>
/// <param name="DailyMessage">Le message du jour, le même pour tout le monde.</param>
public sealed record DashboardSummary(
    DateOnly Today,
    bool WrittenToday,
    WritingStatistics Statistics,
    IReadOnlyList<CalendarWeek> Weeks,
    IReadOnlyList<CalendarDay> RecentDays,
    string DailyMessage);

public sealed record CalendarWeek(IReadOnlyList<CalendarDay> Days);

/// <param name="Date">Le jour.</param>
/// <param name="Written">Vrai si l'utilisateur a écrit ce jour-là.</param>
/// <param name="IsFuture">Vrai si le jour est postérieur à aujourd'hui.</param>
public sealed record CalendarDay(DateOnly Date, bool Written, bool IsFuture);
