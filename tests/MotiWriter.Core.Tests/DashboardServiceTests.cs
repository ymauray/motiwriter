using Microsoft.Extensions.Time.Testing;
using MotiWriter.Core.Dashboard;
using MotiWriter.Core.Journal;

namespace MotiWriter.Core.Tests;

public class DashboardServiceTests
{
    // Dimanche 8 novembre 2026.
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 8);

    private readonly InMemoryWritingJournal _journal = new();
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _service = new DashboardService(_journal, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task Reports_whether_today_is_written()
    {
        Assert.False((await _service.GetSummaryAsync()).WrittenToday);

        await _journal.SetWrittenAsync(Today, true, Now);

        Assert.True((await _service.GetSummaryAsync()).WrittenToday);
    }

    [Fact]
    public async Task Current_week_runs_from_monday_to_sunday_and_ends_the_calendar()
    {
        await _journal.SetWrittenAsync(Today.AddDays(-1), true, Now);

        var summary = await _service.GetSummaryAsync(weeks: 4);

        Assert.Equal(4, summary.Weeks.Count);
        Assert.Same(summary.Weeks[^1], summary.CurrentWeek);
        Assert.Equal(new DateOnly(2026, 11, 2), summary.CurrentWeek.Days[0].Date);
        Assert.Equal(Today, summary.CurrentWeek.Days[6].Date);
        Assert.True(summary.CurrentWeek.Days[5].Written);
    }

    [Fact]
    public async Task Days_after_today_are_flagged_as_future()
    {
        // Mercredi 4 novembre : jeudi → dimanche sont à venir.
        var service = new DashboardService(_journal, new FakeTimeProvider(Now.AddDays(-4)));

        var week = (await service.GetSummaryAsync(weeks: 1)).CurrentWeek;

        Assert.Equal([false, false, false, true, true, true, true], week.Days.Select(d => d.IsFuture));
    }
}
