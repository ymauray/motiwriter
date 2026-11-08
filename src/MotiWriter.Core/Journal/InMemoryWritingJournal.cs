namespace MotiWriter.Core.Journal;

/// <summary>Implémentation en mémoire, destinée aux tests.</summary>
public sealed class InMemoryWritingJournal : IWritingJournal
{
    private readonly HashSet<DateOnly> _days = [];
    private readonly List<WritingDayChange> _history = [];
    private readonly Lock _lock = new();

    public Task<IReadOnlySet<DateOnly>> GetWritingDaysAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlySet<DateOnly>>(new HashSet<DateOnly>(_days));
        }
    }

    public Task<bool> IsWrittenAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_days.Contains(day));
        }
    }

    public Task<bool> SetWrittenAsync(DateOnly day, bool written, DateTimeOffset changedAt, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var changed = written ? _days.Add(day) : _days.Remove(day);
            if (changed)
            {
                _history.Add(new WritingDayChange(day, written, changedAt));
            }
            return Task.FromResult(changed);
        }
    }

    public Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(int limit, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<WritingDayChange> result = _history
                .AsEnumerable()
                .Reverse()
                .Take(limit)
                .ToList();
            return Task.FromResult(result);
        }
    }
}
