using MotiWriter.Core.Accounts;

namespace MotiWriter.Core.Journal;

/// <summary>
/// Stockage des jours d'écriture et de leur historique, propres à chaque compte.
/// Un jour est simplement « écrit » ou « non écrit » : on ne note pas le nombre de mots.
/// </summary>
/// <remarks>
/// Contrat de persistance pur : aucune règle métier ici (voir <see cref="IWritingDayService"/>).
/// </remarks>
public interface IWritingJournal
{
    /// <summary>Retourne tous les jours marqués écrits par ce compte.</summary>
    Task<IReadOnlySet<DateOnly>> GetWritingDaysAsync(UserId user, CancellationToken cancellationToken = default);

    /// <summary>Indique si ce compte a marqué ce jour écrit.</summary>
    Task<bool> IsWrittenAsync(UserId user, DateOnly day, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fixe l'état d'un jour. Si l'état change, une entrée d'historique horodatée
    /// <paramref name="changedAt"/> est enregistrée dans la même opération.
    /// </summary>
    /// <returns>Vrai si l'état a changé, faux s'il était déjà celui demandé.</returns>
    Task<bool> SetWrittenAsync(UserId user, DateOnly day, bool written, DateTimeOffset changedAt, CancellationToken cancellationToken = default);

    /// <summary>Retourne les dernières modifications de ce compte, de la plus récente à la plus ancienne.</summary>
    Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(UserId user, int limit, CancellationToken cancellationToken = default);
}
