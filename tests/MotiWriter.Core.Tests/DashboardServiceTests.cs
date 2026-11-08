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
    public async Task Calendar_ends_with_the_current_week_from_monday_to_sunday()
    {
        var summary = await _service.GetSummaryAsync(weeks: 4);

        Assert.Equal(4, summary.Weeks.Count);
        Assert.Equal(new DateOnly(2026, 11, 2), summary.Weeks[^1].Days[0].Date);
        Assert.Equal(Today, summary.Weeks[^1].Days[6].Date);
    }

    [Fact]
    public async Task Recent_days_are_eight_by_default_and_end_today()
    {
        // Mercredi 4 novembre : du mercredi 28 octobre au mercredi 4 novembre.
        await _journal.SetWrittenAsync(new DateOnly(2026, 10, 28), true, Now);
        var wednesday = new DashboardService(_journal, new FakeTimeProvider(Now.AddDays(-4)));

        var days = (await wednesday.GetSummaryAsync()).RecentDays;

        Assert.Equal(8, days.Count);
        Assert.Equal(new DateOnly(2026, 10, 28), days[0].Date);
        Assert.Equal(new DateOnly(2026, 11, 4), days[^1].Date);
        Assert.True(days[0].Written);
        Assert.All(days, d => Assert.False(d.IsFuture));
    }

    [Fact]
    public async Task Days_after_today_are_flagged_as_future()
    {
        // Mercredi 4 novembre : jeudi → dimanche sont à venir.
        var service = new DashboardService(_journal, new FakeTimeProvider(Now.AddDays(-4)));

        var week = (await service.GetSummaryAsync(weeks: 1)).Weeks[^1];

        Assert.Equal([false, false, false, true, true, true, true], week.Days.Select(d => d.IsFuture));
    }
}
