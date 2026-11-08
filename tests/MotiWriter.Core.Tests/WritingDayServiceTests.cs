using Microsoft.Extensions.Time.Testing;
using MotiWriter.Core.Journal;

namespace MotiWriter.Core.Tests;

public class WritingDayServiceTests
{
    // 18 h 42 UTC = 19 h 42 à UTC+1, fuseau local simulé.
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 18, 42, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 8);

    private readonly InMemoryWritingJournal _journal = new();
    private readonly FakeTimeProvider _clock = new(Now);
    private readonly WritingDayService _service;

    public WritingDayServiceTests()
    {
        _clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        _service = new WritingDayService(_journal, _clock);
    }

    [Fact]
    public async Task Toggle_marks_then_unmarks_a_day()
    {
        var first = await _service.ToggleAsync(Today);
        var second = await _service.ToggleAsync(Today);

        Assert.True(first.Written);
        Assert.False(second.Written);
        Assert.False(await _journal.IsWrittenAsync(Today));
    }

    [Fact]
    public async Task Past_days_can_be_caught_up()
    {
        var change = await _service.ToggleAsync(Today.AddDays(-12));

        Assert.True(change.Written);
        Assert.Contains(Today.AddDays(-12), await _journal.GetWritingDaysAsync());
    }

    [Fact]
    public async Task Future_days_are_refused()
    {
        var tomorrow = Today.AddDays(1);

        await Assert.ThrowsAsync<WritingDayInFutureException>(() => _service.ToggleAsync(tomorrow));
        await Assert.ThrowsAsync<WritingDayInFutureException>(() => _service.SetWrittenAsync(tomorrow, true));
        Assert.Empty(await _journal.GetHistoryAsync(10));
    }

    [Fact]
    public async Task Every_change_is_recorded_with_its_timestamp_newest_first()
    {
        await _service.ToggleAsync(Today.AddDays(-3));
        _clock.Advance(TimeSpan.FromMinutes(5));
        await _service.ToggleAsync(Today.AddDays(-3));

        var history = await _service.GetHistoryAsync();

        Assert.Equal(
            [
                new WritingDayChange(Today.AddDays(-3), false, Now.AddMinutes(5)),
                new WritingDayChange(Today.AddDays(-3), true, Now),
            ],
            history);
    }

    [Fact]
    public async Task Setting_the_current_state_again_records_nothing()
    {
        await _service.SetWrittenAsync(Today, true);

        var again = await _service.SetWrittenAsync(Today, true);
        var unmarkUnwritten = await _service.SetWrittenAsync(Today.AddDays(-1), false);

        Assert.Null(again);
        Assert.Null(unmarkUnwritten);
        Assert.Single(await _service.GetHistoryAsync());
    }

    [Fact]
    public async Task Today_follows_the_local_time_zone()
    {
        // 23 h 30 UTC le 8 novembre = 0 h 30 le 9 novembre à UTC+1.
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 8, 23, 30, 0, TimeSpan.Zero));

        var change = await _service.ToggleAsync(new DateOnly(2026, 11, 9));

        Assert.True(change.Written);
    }

    [Fact]
    public async Task History_is_grouped_by_month_of_change_newest_first()
    {
        // 31 octobre, 23 h 30 à UTC+1 : encore en octobre en heure locale.
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 31, 22, 30, 0, TimeSpan.Zero));
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        var service = new WritingDayService(new InMemoryWritingJournal(), clock);
        await service.ToggleAsync(new DateOnly(2026, 10, 30));
        await service.ToggleAsync(new DateOnly(2026, 10, 31));

        // Une heure plus tard : 1er novembre, 0 h 30 à UTC+1.
        clock.Advance(TimeSpan.FromHours(1));
        await service.ToggleAsync(new DateOnly(2026, 10, 31));

        var months = await service.GetHistoryByMonthAsync();

        Assert.Equal([(2026, 11), (2026, 10)], months.Select(m => (m.Year, m.Month)));
        Assert.Single(months[0].Changes);
        Assert.Equal([new DateOnly(2026, 10, 31), new DateOnly(2026, 10, 30)], months[1].Changes.Select(c => c.Day));
    }
}
