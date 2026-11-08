namespace MotiWriter.Core.Journal;

/// <summary>
/// Implémentation en mémoire, destinée à la maquette et aux tests.
/// Sera remplacée par une persistance réelle (SQLite, etc.).
/// </summary>
public sealed class InMemoryWritingJournal : IWritingJournal
{
    private readonly HashSet<DateOnly> _days = [];
    private readonly Lock _lock = new();

    public InMemoryWritingJournal(IEnumerable<DateOnly>? seed = null)
    {
        if (seed is not null)
        {
            _days.UnionWith(seed);
        }
    }

    public Task<IReadOnlySet<DateOnly>> GetWritingDaysAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlySet<DateOnly>>(new HashSet<DateOnly>(_days));
        }
    }

    public Task MarkAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _days.Add(day);
        }
        return Task.CompletedTask;
    }

    public Task UnmarkAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _days.Remove(day);
        }
        return Task.CompletedTask;
    }
}
