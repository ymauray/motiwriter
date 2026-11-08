namespace MotiWriter.Core.Journal;

/// <summary>Une entrée de l'historique : un jour a été marqué écrit ou non écrit, à un instant donné.</summary>
/// <param name="Day">Le jour concerné.</param>
/// <param name="Written">Nouvel état : vrai si le jour a été marqué écrit, faux s'il a été décoché.</param>
/// <param name="ChangedAt">Date et heure de la modification.</param>
public sealed record WritingDayChange(DateOnly Day, bool Written, DateTimeOffset ChangedAt);
