using MotiWriter.Core.Accounts;

namespace MotiWriter.Core.Journal;

/// <summary>Implémentation en mémoire, destinée aux tests.</summary>
public sealed class InMemoryWritingJournal : IWritingJournal
{
    private readonly HashSet<(UserId User, DateOnly Day)> _days = [];
    private readonly List<(UserId User, WritingDayChange Change)> _history = [];
    private readonly Lock _lock = new();

    public Task<IReadOnlySet<DateOnly>> GetWritingDaysAsync(UserId user, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlySet<DateOnly>>(
                _days.Where(d => d.User == user).Select(d => d.Day).ToHashSet());
        }
    }

    public Task<bool> IsWrittenAsync(UserId user, DateOnly day, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_days.Contains((user, day)));
        }
    }

    public Task<bool> SetWrittenAsync(UserId user, DateOnly day, bool written, DateTimeOffset changedAt, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var changed = written ? _days.Add((user, day)) : _days.Remove((user, day));
            if (changed)
            {
                _history.Add((user, new WritingDayChange(day, written, changedAt)));
            }
            return Task.FromResult(changed);
        }
    }

    public Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(UserId user, int limit, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<WritingDayChange> result = _history
                .Where(h => h.User == user)
                .Select(h => h.Change)
                .Reverse()
                .Take(limit)
                .ToList();
            return Task.FromResult(result);
        }
    }
}
