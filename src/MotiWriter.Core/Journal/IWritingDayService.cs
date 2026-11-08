namespace MotiWriter.Core.Journal;

/// <summary>Saisie des jours d'écriture : cocher, décocher, consulter l'historique.</summary>
public interface IWritingDayService
{
    /// <summary>Fixe l'état d'un jour (aujourd'hui ou passé).</summary>
    /// <returns>La modification enregistrée, ou <c>null</c> si le jour était déjà dans cet état.</returns>
    /// <exception cref="WritingDayInFutureException">Le jour est postérieur à aujourd'hui.</exception>
    Task<WritingDayChange?> SetWrittenAsync(DateOnly day, bool written, CancellationToken cancellationToken = default);

    /// <summary>Inverse l'état d'un jour (aujourd'hui ou passé).</summary>
    /// <exception cref="WritingDayInFutureException">Le jour est postérieur à aujourd'hui.</exception>
    Task<WritingDayChange> ToggleAsync(DateOnly day, CancellationToken cancellationToken = default);

    /// <summary>Dernières modifications, de la plus récente à la plus ancienne.</summary>
    Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(int limit = 50, CancellationToken cancellationToken = default);
}
