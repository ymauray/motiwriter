using Microsoft.Data.Sqlite;

namespace MotiWriter.Infrastructure.Sqlite;

/// <summary>
/// Création et évolution du schéma. La version courante est stockée dans
/// <c>PRAGMA user_version</c> ; chaque migration fait passer à la version suivante.
/// Pour faire évoluer le schéma : ajouter une entrée à <see cref="Migrations"/>, jamais modifier les existantes.
/// </summary>
internal static class SqliteSchema
{
    private static readonly string[] Migrations =
    [
        // Version 1 : jours écrits et historique des modifications.
        """
        CREATE TABLE writing_day (
            day TEXT NOT NULL PRIMARY KEY           -- AAAA-MM-JJ
        ) WITHOUT ROWID;

        CREATE TABLE writing_day_change (
            id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            day        TEXT    NOT NULL,            -- AAAA-MM-JJ
            written    INTEGER NOT NULL,            -- 1 = écrit, 0 = non écrit
            changed_at TEXT    NOT NULL             -- ISO 8601 avec décalage horaire
        );
        """,
    ];

    public static int CurrentVersion => Migrations.Length;

    public static async Task MigrateAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using (var wal = connection.CreateCommand())
        {
            wal.CommandText = "PRAGMA journal_mode = WAL";
            await wal.ExecuteNonQueryAsync(cancellationToken);
        }

        var version = await GetVersionAsync(connection, cancellationToken);
        for (var target = version + 1; target <= Migrations.Length; target++)
        {
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"{Migrations[target - 1]}\nPRAGMA user_version = {target};";
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }

    public static async Task<int> GetVersionAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version";
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }
}
