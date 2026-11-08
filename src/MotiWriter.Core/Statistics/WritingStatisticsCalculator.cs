namespace MotiWriter.Core.Statistics;

/// <summary>Calcul pur (sans effet de bord) des statistiques d'écriture.</summary>
public static class WritingStatisticsCalculator
{
    public static WritingStatistics Compute(IReadOnlySet<DateOnly> days, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(days);

        var past = days.Where(d => d <= today).ToHashSet();

        return new WritingStatistics(
            TotalDays: past.Count,
            CurrentStreak: ComputeCurrentStreak(past, today),
            LongestStreak: ComputeLongestStreak(past),
            DaysThisMonth: past.Count(d => d.Year == today.Year && d.Month == today.Month));
    }

    /// <summary>
    /// La série reste « vivante » tant que la journée n'est pas terminée :
    /// si aujourd'hui n'est pas encore écrit, on compte à partir d'hier.
    /// </summary>
    private static int ComputeCurrentStreak(HashSet<DateOnly> days, DateOnly today)
    {
        var cursor = days.Contains(today) ? today : today.AddDays(-1);
        var streak = 0;
        while (days.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return streak;
    }

    private static int ComputeLongestStreak(HashSet<DateOnly> days)
    {
        var longest = 0;
        foreach (var day in days)
        {
            // On ne démarre un comptage qu'au début d'une série.
            if (days.Contains(day.AddDays(-1)))
            {
                continue;
            }

            var length = 1;
            while (days.Contains(day.AddDays(length)))
            {
                length++;
            }
            longest = Math.Max(longest, length);
        }
        return longest;
    }
}
