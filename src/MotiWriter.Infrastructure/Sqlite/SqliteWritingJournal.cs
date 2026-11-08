using System.Globalization;
using Microsoft.Data.Sqlite;
using MotiWriter.Core.Accounts;
using MotiWriter.Core.Journal;

namespace MotiWriter.Infrastructure.Sqlite;

/// <summary>Journal d'écriture stocké dans SQLite, une ligne par compte et par jour écrit.</summary>
public sealed class SqliteWritingJournal(SqliteDatabase database) : IWritingJournal
{
    private const string DayFormat = "yyyy-MM-dd";
    private const string TimestampFormat = "O"; // écriture en ISO 8601 avec décalage ; lecture tolérante à toute forme ISO 8601

    /// <summary>Raccourci pour les tests et les outils : base dédiée à ce journal.</summary>
    public SqliteWritingJournal(string connectionString) : this(new SqliteDatabase(connectionString))
    {
    }

    public async Task<IReadOnlySet<DateOnly>> GetWritingDaysAsync(UserId user, CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT day FROM writing_day WHERE user_id = $user";
        command.Parameters.AddWithValue("$user", user.Value);

        var days = new HashSet<DateOnly>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            days.Add(ParseDay(reader.GetString(0)));
        }
        return days;
    }

    public async Task<bool> IsWrittenAsync(UserId user, DateOnly day, CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS (SELECT 1 FROM writing_day WHERE user_id = $user AND day = $day)";
        command.Parameters.AddWithValue("$user", user.Value);
        command.Parameters.AddWithValue("$day", FormatDay(day));

        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    public async Task<bool> SetWrittenAsync(UserId user, DateOnly day, bool written, DateTimeOffset changedAt, CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await using var update = connection.CreateCommand();
        update.Transaction = transaction;
        update.CommandText = written
            ? "INSERT OR IGNORE INTO writing_day (user_id, day) VALUES ($user, $day)"
            : "DELETE FROM writing_day WHERE user_id = $user AND day = $day";
        update.Parameters.AddWithValue("$user", user.Value);
        update.Parameters.AddWithValue("$day", FormatDay(day));

        var changed = await update.ExecuteNonQueryAsync(cancellationToken) > 0;
        if (changed)
        {
            await using var log = connection.CreateCommand();
            log.Transaction = transaction;
            log.CommandText = """
                INSERT INTO writing_day_change (user_id, day, written, changed_at)
                VALUES ($user, $day, $written, $changedAt)
                """;
            log.Parameters.AddWithValue("$user", user.Value);
            log.Parameters.AddWithValue("$day", FormatDay(day));
            log.Parameters.AddWithValue("$written", written ? 1 : 0);
            log.Parameters.AddWithValue("$changedAt", changedAt.ToString(TimestampFormat, CultureInfo.InvariantCulture));
            await log.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return changed;
    }

    public async Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(UserId user, int limit, CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT day, written, changed_at
            FROM writing_day_change
            WHERE user_id = $user
            ORDER BY id DESC
            LIMIT $limit
            """;
        command.Parameters.AddWithValue("$user", user.Value);
        command.Parameters.AddWithValue("$limit", limit);

        var history = new List<WritingDayChange>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            history.Add(new WritingDayChange(
                ParseDay(reader.GetString(0)),
                reader.GetInt64(1) == 1,
                DateTimeOffset.Parse(reader.GetString(2), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)));
        }
        return history;
    }

    private static string FormatDay(DateOnly day) => day.ToString(DayFormat, CultureInfo.InvariantCulture);

    private static DateOnly ParseDay(string value) => DateOnly.ParseExact(value, DayFormat, CultureInfo.InvariantCulture);
}
