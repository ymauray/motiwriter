using MotiWriter.Core.Accounts;
using MotiWriter.Core.Journal;
using MotiWriter.Infrastructure.Sqlite;

namespace MotiWriter.Infrastructure.Tests;

public sealed class SqliteWritingJournalTests : IDisposable
{
    private static readonly UserId Alice = new(1);

    private static readonly DateOnly Day = new(2026, 11, 3);
    private static readonly DateTimeOffset At = new(2026, 11, 8, 19, 42, 5, TimeSpan.FromHours(1));

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"motiwriter-tests-{Guid.NewGuid():N}");
    private readonly string _connectionString;

    public SqliteWritingJournalTests()
    {
        Directory.CreateDirectory(_directory);
        // Pooling désactivé pour pouvoir supprimer le fichier à la fin.
        _connectionString = $"Data Source={Path.Combine(_directory, "test.db")};Pooling=False";
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private SqliteWritingJournal NewJournal() => new(_connectionString);

    [Fact]
    public async Task Empty_database_is_created_on_first_access()
    {
        var journal = NewJournal();

        Assert.Empty(await journal.GetWritingDaysAsync(Alice));
        Assert.Empty(await journal.GetHistoryAsync(Alice, 10));
    }

    [Fact]
    public async Task Days_and_history_survive_a_new_instance()
    {
        await NewJournal().SetWrittenAsync(Alice, Day, true, At);

        var reopened = NewJournal();

        Assert.True(await reopened.IsWrittenAsync(Alice, Day));
        Assert.Equal([Day], await reopened.GetWritingDaysAsync(Alice));
        Assert.Equal([new WritingDayChange(Day, true, At)], await reopened.GetHistoryAsync(Alice, 10));
    }

    [Fact]
    public async Task Unmarking_removes_the_day_and_is_recorded()
    {
        var journal = NewJournal();
        await journal.SetWrittenAsync(Alice, Day, true, At);

        var changed = await journal.SetWrittenAsync(Alice, Day, false, At.AddMinutes(1));

        Assert.True(changed);
        Assert.False(await journal.IsWrittenAsync(Alice, Day));
        Assert.Equal(
            [new WritingDayChange(Day, false, At.AddMinutes(1)), new WritingDayChange(Day, true, At)],
            await journal.GetHistoryAsync(Alice, 10));
    }

    [Fact]
    public async Task Setting_the_same_state_twice_changes_nothing()
    {
        var journal = NewJournal();

        Assert.True(await journal.SetWrittenAsync(Alice, Day, true, At));
        Assert.False(await journal.SetWrittenAsync(Alice, Day, true, At));
        Assert.False(await journal.SetWrittenAsync(Alice, Day.AddDays(-1), false, At));

        Assert.Single(await journal.GetHistoryAsync(Alice, 10));
    }

    [Fact]
    public async Task History_is_limited_and_newest_first()
    {
        var journal = NewJournal();
        for (var i = 0; i < 5; i++)
        {
            await journal.SetWrittenAsync(Alice, Day.AddDays(-i), true, At.AddMinutes(i));
        }

        var history = await journal.GetHistoryAsync(Alice, 2);

        Assert.Equal([Day.AddDays(-4), Day.AddDays(-3)], history.Select(c => c.Day));
    }
}
