namespace MotiWriter.Core.Journal;

/// <summary>
/// Journal des jours d'écriture. Pour cette première version, on ne note pas
/// le nombre de mots : un jour est simplement « écrit » ou « non écrit ».
/// </summary>
public interface IWritingJournal
{
    /// <summary>Retourne tous les jours où l'utilisateur a écrit.</summary>
    Task<IReadOnlySet<DateOnly>> GetWritingDaysAsync(CancellationToken cancellationToken = default);

    /// <summary>Marque un jour comme écrit (idempotent).</summary>
    Task MarkAsync(DateOnly day, CancellationToken cancellationToken = default);

    /// <summary>Retire la marque d'un jour (idempotent).</summary>
    Task UnmarkAsync(DateOnly day, CancellationToken cancellationToken = default);
}
