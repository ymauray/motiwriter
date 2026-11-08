using MotiWriter.Core.Accounts;
namespace MotiWriter.Core.Journal;

/// <summary>Saisie des jours d'écriture : cocher, décocher, consulter l'historique.</summary>
public interface IWritingDayService
{
    /// <summary>Fixe l'état d'un jour (aujourd'hui ou passé).</summary>
    /// <returns>La modification enregistrée, ou <c>null</c> si le jour était déjà dans cet état.</returns>
    /// <exception cref="WritingDayInFutureException">Le jour est postérieur à aujourd'hui.</exception>
    Task<WritingDayChange?> SetWrittenAsync(UserId user, DateOnly day, bool written, CancellationToken cancellationToken = default);

    /// <summary>Inverse l'état d'un jour (aujourd'hui ou passé).</summary>
    /// <exception cref="WritingDayInFutureException">Le jour est postérieur à aujourd'hui.</exception>
    Task<WritingDayChange> ToggleAsync(UserId user, DateOnly day, CancellationToken cancellationToken = default);

    /// <summary>Dernières modifications, de la plus récente à la plus ancienne.</summary>
    Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(UserId user, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Historique regroupé par mois de modification (selon l'heure locale enregistrée),
    /// du mois le plus récent au plus ancien.
    /// </summary>
    Task<IReadOnlyList<HistoryMonth>> GetHistoryByMonthAsync(UserId user, int limit = 10_000, CancellationToken cancellationToken = default);
}
