using MotiWriter.Core.Statistics;

namespace MotiWriter.Core.Tests;

public class WritingStatisticsCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 8);

    private static HashSet<DateOnly> DaysAgo(params int[] offsets) =>
        offsets.Select(o => Today.AddDays(-o)).ToHashSet();

    [Fact]
    public void Empty_journal_yields_zeroes()
    {
        var stats = WritingStatisticsCalculator.Compute(new HashSet<DateOnly>(), Today);

        Assert.Equal(new WritingStatistics(0, 0, 0, 0), stats);
    }

    [Fact]
    public void Current_streak_includes_today()
    {
        var stats = WritingStatisticsCalculator.Compute(DaysAgo(0, 1, 2, 4), Today);

        Assert.Equal(3, stats.CurrentStreak);
    }

    [Fact]
    public void Current_streak_stays_alive_when_today_not_yet_written()
    {
        var stats = WritingStatisticsCalculator.Compute(DaysAgo(1, 2), Today);

        Assert.Equal(2, stats.CurrentStreak);
    }

    [Fact]
    public void Current_streak_is_broken_after_a_missed_day()
    {
        var stats = WritingStatisticsCalculator.Compute(DaysAgo(2, 3, 4), Today);

        Assert.Equal(0, stats.CurrentStreak);
        Assert.Equal(3, stats.LongestStreak);
    }

    [Fact]
    public void Counts_days_of_current_month_and_ignores_future()
    {
        var days = DaysAgo(0, 7, 8, 20); // 8, 1 nov., 31 oct., 19 oct.
        days.Add(Today.AddDays(3));

        var stats = WritingStatisticsCalculator.Compute(days, Today);

        Assert.Equal(4, stats.TotalDays);
        Assert.Equal(2, stats.DaysThisMonth);
    }
}
