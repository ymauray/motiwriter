namespace MotiWriter.Core.Journal;

/// <summary>
/// Stockage des jours d'écriture et de leur historique. Pour cette première version,
/// on ne note pas le nombre de mots : un jour est simplement « écrit » ou « non écrit ».
/// </summary>
/// <remarks>
/// Contrat de persistance pur : aucune règle métier ici (voir <see cref="IWritingDayService"/>).
/// </remarks>
public interface IWritingJournal
{
    /// <summary>Retourne tous les jours marqués écrits.</summary>
    Task<IReadOnlySet<DateOnly>> GetWritingDaysAsync(CancellationToken cancellationToken = default);

    /// <summary>Indique si un jour est marqué écrit.</summary>
    Task<bool> IsWrittenAsync(DateOnly day, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fixe l'état d'un jour. Si l'état change, une entrée d'historique horodatée
    /// <paramref name="changedAt"/> est enregistrée dans la même opération.
    /// </summary>
    /// <returns>Vrai si l'état a changé, faux s'il était déjà celui demandé.</returns>
    Task<bool> SetWrittenAsync(DateOnly day, bool written, DateTimeOffset changedAt, CancellationToken cancellationToken = default);

    /// <summary>Retourne les dernières modifications, de la plus récente à la plus ancienne.</summary>
    Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(int limit, CancellationToken cancellationToken = default);
}
