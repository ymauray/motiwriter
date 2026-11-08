namespace MotiWriter.Core.Journal;

/// <summary>Levée quand on tente de marquer un jour qui n'est pas encore arrivé.</summary>
public sealed class WritingDayInFutureException(DateOnly day, DateOnly today)
    : InvalidOperationException($"Le jour {day:yyyy-MM-dd} est postérieur à aujourd'hui ({today:yyyy-MM-dd}).")
{
    public DateOnly Day { get; } = day;
    public DateOnly Today { get; } = today;
}
