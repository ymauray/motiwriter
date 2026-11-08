using Microsoft.Data.Sqlite;

namespace MotiWriter.Infrastructure.Sqlite;

/// <summary>
/// Accès au fichier SQLite partagé par les différents stockages.
/// Le schéma est créé (ou mis à jour) automatiquement à la première ouverture.
/// </summary>
public sealed class SqliteDatabase(string connectionString)
{
    private readonly SemaphoreSlim _schemaLock = new(1, 1);
    private volatile bool _schemaReady;

    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = ON";
            await pragma.ExecuteNonQueryAsync(cancellationToken);
        }

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
}
