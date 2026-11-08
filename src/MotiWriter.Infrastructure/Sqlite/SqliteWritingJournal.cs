using System.Globalization;
using Microsoft.Data.Sqlite;
using MotiWriter.Core.Journal;

namespace MotiWriter.Infrastructure.Sqlite;

/// <summary>
/// Journal d'écriture stocké dans un fichier SQLite.
/// Le schéma est créé (ou mis à jour) automatiquement au premier accès.
/// </summary>
public sealed class SqliteWritingJournal(string connectionString) : IWritingJournal
{
    private const string DayFormat = "yyyy-MM-dd";
    private const string TimestampFormat = "O"; // écriture en ISO 8601 avec décalage ; lecture tolérante à toute forme ISO 8601

    private readonly SemaphoreSlim _schemaLock = new(1, 1);
    private volatile bool _schemaReady;

    public async Task<IReadOnlySet<DateOnly>> GetWritingDaysAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT day FROM writing_day";

        var days = new HashSet<DateOnly>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            days.Add(ParseDay(reader.GetString(0)));
        }
        return days;
    }

    public async Task<bool> IsWrittenAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS (SELECT 1 FROM writing_day WHERE day = $day)";
        command.Parameters.AddWithValue("$day", FormatDay(day));

        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    public async Task<bool> SetWrittenAsync(DateOnly day, bool written, DateTimeOffset changedAt, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await using var update = connection.CreateCommand();
        update.Transaction = transaction;
        update.CommandText = written
            ? "INSERT OR IGNORE INTO writing_day (day) VALUES ($day)"
            : "DELETE FROM writing_day WHERE day = $day";
        update.Parameters.AddWithValue("$day", FormatDay(day));

        var changed = await update.ExecuteNonQueryAsync(cancellationToken) > 0;
        if (changed)
        {
            await using var log = connection.CreateCommand();
            log.Transaction = transaction;
            log.CommandText = """
                INSERT INTO writing_day_change (day, written, changed_at)
                VALUES ($day, $written, $changedAt)
                """;
            log.Parameters.AddWithValue("$day", FormatDay(day));
            log.Parameters.AddWithValue("$written", written ? 1 : 0);
            log.Parameters.AddWithValue("$changedAt", changedAt.ToString(TimestampFormat, CultureInfo.InvariantCulture));
            await log.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return changed;
    }

    public async Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(int limit, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT day, written, changed_at
            FROM writing_day_change
            ORDER BY id DESC
            LIMIT $limit
            """;
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

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        if (!_schemaReady)
        {
            await EnsureSchemaAsync(connection, cancellationToken);
        }
        return connection;
    }

    private async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await _schemaLock.WaitAsync(cancellationToken);
        try
        {
            if (_schemaReady)
            {
                return;
            }
            await SqliteSchema.MigrateAsync(connection, cancellationToken);
            _schemaReady = true;
        }
        finally
        {
            _schemaLock.Release();
        }
    }

    private static string FormatDay(DateOnly day) => day.ToString(DayFormat, CultureInfo.InvariantCulture);

    private static DateOnly ParseDay(string value) => DateOnly.ParseExact(value, DayFormat, CultureInfo.InvariantCulture);
}
