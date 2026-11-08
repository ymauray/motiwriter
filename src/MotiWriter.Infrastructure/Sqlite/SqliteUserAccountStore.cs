using System.Globalization;
using Microsoft.Data.Sqlite;
using MotiWriter.Core.Accounts;

namespace MotiWriter.Infrastructure.Sqlite;

/// <summary>Comptes et connexions OAuth stockés dans SQLite.</summary>
public sealed class SqliteUserAccountStore(SqliteDatabase database) : IUserAccountStore
{
    private const string TimestampFormat = "O";

    public async Task<UserAccount?> FindByLoginAsync(string provider, string subject, CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT a.id, a.display_name, a.email, a.created_at
            FROM external_login l
            JOIN user_account a ON a.id = l.user_id
            WHERE l.provider = $provider AND l.subject = $subject
            """;
        command.Parameters.AddWithValue("$provider", provider);
        command.Parameters.AddWithValue("$subject", subject);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }
        return new UserAccount(
            new UserId(reader.GetInt64(0)),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            DateTimeOffset.Parse(reader.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
    }

    /// <remarks>
    /// Le tout premier compte créé adopte les données saisies avant l'arrivée des comptes
    /// (celles du schéma version 1), pour que la personne qui utilisait déjà l'application les retrouve.
    /// </remarks>
    public async Task<UserAccount> CreateWithLoginAsync(
        string displayName, string? email, string provider, string subject, DateTimeOffset createdAt,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        long existingAccounts;
        await using (var count = connection.CreateCommand())
        {
            count.Transaction = transaction;
            count.CommandText = "SELECT COUNT(*) FROM user_account";
            existingAccounts = Convert.ToInt64(await count.ExecuteScalarAsync(cancellationToken));
        }

        long id;
        await using (var insert = connection.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO user_account (display_name, email, created_at)
                VALUES ($name, $email, $createdAt)
                RETURNING id
                """;
            insert.Parameters.AddWithValue("$name", displayName);
            insert.Parameters.AddWithValue("$email", (object?)email ?? DBNull.Value);
            insert.Parameters.AddWithValue("$createdAt", createdAt.ToString(TimestampFormat, CultureInfo.InvariantCulture));
            id = Convert.ToInt64(await insert.ExecuteScalarAsync(cancellationToken));
        }

        await using (var login = connection.CreateCommand())
        {
            login.Transaction = transaction;
            login.CommandText = "INSERT INTO external_login (provider, subject, user_id) VALUES ($provider, $subject, $user)";
            login.Parameters.AddWithValue("$provider", provider);
            login.Parameters.AddWithValue("$subject", subject);
            login.Parameters.AddWithValue("$user", id);
            await login.ExecuteNonQueryAsync(cancellationToken);
        }

        if (existingAccounts == 0)
        {
            await using var adopt = connection.CreateCommand();
            adopt.Transaction = transaction;
            adopt.CommandText = """
                UPDATE writing_day SET user_id = $user WHERE user_id = $unclaimed;
                UPDATE writing_day_change SET user_id = $user WHERE user_id = $unclaimed;
                """;
            adopt.Parameters.AddWithValue("$user", id);
            adopt.Parameters.AddWithValue("$unclaimed", SqliteSchema.UnclaimedUserId);
            await adopt.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return new UserAccount(new UserId(id), displayName, email, createdAt);
    }

    public async Task UpdateProfileAsync(UserId id, string displayName, string? email, CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE user_account SET display_name = $name, email = $email WHERE id = $id";
        command.Parameters.AddWithValue("$name", displayName);
        command.Parameters.AddWithValue("$email", (object?)email ?? DBNull.Value);
        command.Parameters.AddWithValue("$id", id.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
