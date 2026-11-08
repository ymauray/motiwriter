using Microsoft.Data.Sqlite;

namespace MotiWriter.Infrastructure.Sqlite;

/// <summary>
/// Création et évolution du schéma. La version courante est stockée dans
/// <c>PRAGMA user_version</c> ; chaque migration fait passer à la version suivante.
/// Pour faire évoluer le schéma : ajouter une entrée à <see cref="Migrations"/>, jamais modifier les existantes.
/// </summary>
internal static class SqliteSchema
{
    /// <summary>
    /// Propriétaire des données saisies avant l'arrivée des comptes (schéma version 1).
    /// Elles sont adoptées par le tout premier compte créé.
    /// </summary>
    public const long UnclaimedUserId = 0;

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

        // Version 2 : comptes, connexions OAuth, et données propres à chaque compte.
        // Les données existantes sont rattachées au compte 0 (« non réclamées »).
        """
        CREATE TABLE user_account (
            id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            display_name TEXT    NOT NULL,
            email        TEXT,
            created_at   TEXT    NOT NULL            -- ISO 8601 avec décalage horaire
        );

        CREATE TABLE external_login (
            provider TEXT    NOT NULL,              -- Google, Discord, Twitch, Apple
            subject  TEXT    NOT NULL,              -- identifiant stable chez le fournisseur
            user_id  INTEGER NOT NULL REFERENCES user_account (id) ON DELETE CASCADE,
            PRIMARY KEY (provider, subject)
        ) WITHOUT ROWID;

        CREATE INDEX ix_external_login_user ON external_login (user_id);

        CREATE TABLE writing_day_v2 (
            user_id INTEGER NOT NULL,
            day     TEXT    NOT NULL,
            PRIMARY KEY (user_id, day)
        ) WITHOUT ROWID;

        INSERT INTO writing_day_v2 (user_id, day) SELECT 0, day FROM writing_day;
        DROP TABLE writing_day;
        ALTER TABLE writing_day_v2 RENAME TO writing_day;

        ALTER TABLE writing_day_change ADD COLUMN user_id INTEGER NOT NULL DEFAULT 0;
        CREATE INDEX ix_writing_day_change_user ON writing_day_change (user_id, id);
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
