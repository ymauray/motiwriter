using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MotiWriter.Core.Journal;
using MotiWriter.Infrastructure.Sqlite;

namespace MotiWriter.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre le journal d'écriture SQLite.
    /// </summary>
    /// <param name="connectionString">Chaîne de connexion SQLite, par exemple <c>Data Source=App_Data/motiwriter.db</c>.</param>
    /// <param name="baseDirectory">
    /// Dossier de référence pour un chemin de fichier relatif (en général la racine du contenu de l'application).
    /// Le dossier du fichier est créé s'il n'existe pas.
    /// </param>
    public static IServiceCollection AddMotiWriterSqlite(this IServiceCollection services, string connectionString, string baseDirectory)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var isFile = builder.Mode != SqliteOpenMode.Memory
                     && !string.IsNullOrEmpty(builder.DataSource)
                     && builder.DataSource != ":memory:";

        if (isFile)
        {
            builder.DataSource = Path.GetFullPath(builder.DataSource, baseDirectory);
            var directory = Path.GetDirectoryName(builder.DataSource);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        var resolved = builder.ToString();
        services.AddSingleton<IWritingJournal>(_ => new SqliteWritingJournal(resolved));
        return services;
    }
}
