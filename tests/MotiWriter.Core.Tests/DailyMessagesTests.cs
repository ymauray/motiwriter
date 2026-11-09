using MotiWriter.Core.Messages;

namespace MotiWriter.Core.Tests;

public class DailyMessagesTests
{
    private static readonly DateOnly Start = new(2026, 11, 9);

    [Fact]
    public void Same_day_gives_the_same_message()
    {
        Assert.Equal(DailyMessages.For(Start), DailyMessages.For(new DateOnly(2026, 11, 9)));
    }

    [Fact]
    public void Every_message_appears_once_per_cycle()
    {
        var count = DailyMessages.All.Count;

        var cycle = Enumerable.Range(0, count).Select(i => DailyMessages.For(Start.AddDays(i))).ToList();

        Assert.Equal(DailyMessages.All.OrderBy(m => m), cycle.OrderBy(m => m));
    }

    [Fact]
    public void Consecutive_days_get_different_messages()
    {
        for (var i = 0; i < 400; i++)
        {
            Assert.NotEqual(DailyMessages.For(Start.AddDays(i)), DailyMessages.For(Start.AddDays(i + 1)));
        }
    }

    [Fact]
    public void Messages_are_unique_and_need_no_non_breaking_space()
    {
        Assert.Equal(DailyMessages.All.Count, DailyMessages.All.Distinct().Count());
        Assert.All(DailyMessages.All, m => Assert.DoesNotMatch("[?!;:]", m));
    }
}
