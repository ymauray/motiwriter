namespace MotiWriter.Core.Statistics;

/// <summary>Statistiques calculées à partir des jours d'écriture.</summary>
/// <param name="TotalDays">Nombre total de jours écrits.</param>
/// <param name="CurrentStreak">Série en cours (jours consécutifs jusqu'à aujourd'hui ou hier).</param>
/// <param name="LongestStreak">Plus longue série jamais réalisée.</param>
/// <param name="DaysThisMonth">Jours écrits depuis le début du mois courant.</param>
public sealed record WritingStatistics(
    int TotalDays,
    int CurrentStreak,
    int LongestStreak,
    int DaysThisMonth);
