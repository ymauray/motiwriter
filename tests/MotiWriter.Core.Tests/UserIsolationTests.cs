using Microsoft.Extensions.Time.Testing;
using MotiWriter.Core.Accounts;
using MotiWriter.Core.Dashboard;
using MotiWriter.Core.Journal;

namespace MotiWriter.Core.Tests;

public class UserIsolationTests
{
    private static readonly UserId Alice = new(1);
    private static readonly UserId Bob = new(2);
    private static readonly DateOnly Today = new(2026, 11, 8);

    private readonly InMemoryWritingJournal _journal = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 8, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Each_account_only_sees_its_own_days_and_history()
    {
        var days = new WritingDayService(_journal, _clock);
        var dashboard = new DashboardService(_journal, _clock);

        await days.ToggleAsync(Alice, Today);
        await days.ToggleAsync(Alice, Today.AddDays(-1));
        await days.ToggleAsync(Bob, Today.AddDays(-5));

        Assert.Equal(2, (await dashboard.GetSummaryAsync(Alice)).Statistics.TotalDays);
        Assert.Equal(1, (await dashboard.GetSummaryAsync(Bob)).Statistics.TotalDays);
        Assert.Equal([Today.AddDays(-5)], (await days.GetHistoryAsync(Bob)).Select(c => c.Day));
        Assert.False((await dashboard.GetSummaryAsync(Bob)).WrittenToday);
    }

    [Fact]
    public async Task Toggling_the_same_day_for_two_accounts_is_independent()
    {
        var days = new WritingDayService(_journal, _clock);

        await days.ToggleAsync(Alice, Today);
        var bob = await days.ToggleAsync(Bob, Today);

        Assert.True(bob.Written);
        Assert.True(await _journal.IsWrittenAsync(Alice, Today));
    }
}
