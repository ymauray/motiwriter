using Microsoft.Data.Sqlite;
using MotiWriter.Core.Accounts;
using MotiWriter.Infrastructure.Sqlite;

namespace MotiWriter.Infrastructure.Tests;

public sealed class SqliteUserAccountStoreTests : IDisposable
{
    private static readonly DateTimeOffset At = new(2026, 11, 8, 19, 42, 5, TimeSpan.FromHours(1));

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"motiwriter-tests-{Guid.NewGuid():N}");
    private readonly string _connectionString;

    public SqliteUserAccountStoreTests()
    {
        Directory.CreateDirectory(_directory);
        _connectionString = $"Data Source={Path.Combine(_directory, "test.db")};Pooling=False";
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Fact]
    public async Task Account_is_found_again_by_its_login()
    {
        var store = new SqliteUserAccountStore(new SqliteDatabase(_connectionString));

        var created = await store.CreateWithLoginAsync("Yannick", null, "Discord", "42", At);
        var found = await new SqliteUserAccountStore(new SqliteDatabase(_connectionString)).FindByLoginAsync("Discord", "42");

        Assert.Equal(created, found);
        Assert.Null(await store.FindByLoginAsync("Twitch", "42"));
    }

    [Fact]
    public async Task Profile_can_be_updated()
    {
        var store = new SqliteUserAccountStore(new SqliteDatabase(_connectionString));
        var created = await store.CreateWithLoginAsync("Yannick", null, "Google", "abc", At);

        await store.UpdateProfileAsync(created.Id, "Yannick M.", "y@example.org");

        Assert.Equal(created with { DisplayName = "Yannick M.", Email = "y@example.org" }, await store.FindByLoginAsync("Google", "abc"));
    }

    [Fact]
    public async Task A_login_cannot_belong_to_two_accounts()
    {
        var store = new SqliteUserAccountStore(new SqliteDatabase(_connectionString));
        await store.CreateWithLoginAsync("A", null, "Apple", "001", At);

        await Assert.ThrowsAsync<SqliteException>(() => store.CreateWithLoginAsync("B", null, "Apple", "001", At));
    }

    [Fact]
    public async Task Data_from_before_accounts_is_adopted_by_the_first_account_only()
    {
        // Base au schéma version 1, comme avant l'arrivée des comptes.
        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE writing_day (day TEXT NOT NULL PRIMARY KEY) WITHOUT ROWID;
                CREATE TABLE writing_day_change (
                    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, day TEXT NOT NULL,
                    written INTEGER NOT NULL, changed_at TEXT NOT NULL);
                INSERT INTO writing_day VALUES ('2026-11-01'), ('2026-11-02');
                INSERT INTO writing_day_change (day, written, changed_at) VALUES
                    ('2026-11-01', 1, '2026-11-01T21:00:00.0000000+01:00'),
                    ('2026-11-02', 1, '2026-11-02T21:00:00.0000000+01:00');
                PRAGMA user_version = 1;
                """;
            await command.ExecuteNonQueryAsync();
        }

        var database = new SqliteDatabase(_connectionString);
        var store = new SqliteUserAccountStore(database);
        var journal = new SqliteWritingJournal(database);

        var first = await store.CreateWithLoginAsync("Propriétaire", null, "Discord", "1", At);
        var second = await store.CreateWithLoginAsync("Ami", null, "Twitch", "2", At);

        Assert.Equal([new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 2)], (await journal.GetWritingDaysAsync(first.Id)).Order());
        Assert.Equal(2, (await journal.GetHistoryAsync(first.Id, 10)).Count);
        Assert.Empty(await journal.GetWritingDaysAsync(second.Id));
        Assert.Empty(await journal.GetHistoryAsync(second.Id, 10));
    }

    [Fact]
    public async Task Journals_of_two_accounts_are_isolated()
    {
        var database = new SqliteDatabase(_connectionString);
        var journal = new SqliteWritingJournal(database);
        var day = new DateOnly(2026, 11, 3);

        await journal.SetWrittenAsync(new UserId(1), day, true, At);

        Assert.False(await journal.IsWrittenAsync(new UserId(2), day));
        Assert.True(await journal.SetWrittenAsync(new UserId(2), day, true, At));
        Assert.Single(await journal.GetHistoryAsync(new UserId(1), 10));
    }
}
