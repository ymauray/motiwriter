namespace MotiWriter.Core.Journal;

/// <summary>Les modifications effectuées pendant un mois donné, de la plus récente à la plus ancienne.</summary>
/// <param name="Year">Année de la modification.</param>
/// <param name="Month">Mois de la modification (1 à 12).</param>
/// <param name="Changes">Modifications du mois, de la plus récente à la plus ancienne.</param>
public sealed record HistoryMonth(int Year, int Month, IReadOnlyList<WritingDayChange> Changes)
{
    public bool Is(DateOnly day) => day.Year == Year && day.Month == Month;
}
