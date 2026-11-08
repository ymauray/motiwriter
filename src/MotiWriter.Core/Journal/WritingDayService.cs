using MotiWriter.Core.Accounts;
namespace MotiWriter.Core.Journal;

public sealed class WritingDayService(IWritingJournal journal, TimeProvider timeProvider) : IWritingDayService
{
    public async Task<WritingDayChange?> SetWrittenAsync(UserId user, DateOnly day, bool written, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetLocalNow();
        EnsureNotInFuture(day, now);

        var changed = await journal.SetWrittenAsync(user, day, written, now, cancellationToken);
        return changed ? new WritingDayChange(day, written, now) : null;
    }

    public async Task<WritingDayChange> ToggleAsync(UserId user, DateOnly day, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetLocalNow();
        EnsureNotInFuture(day, now);

        var written = !await journal.IsWrittenAsync(user, day, cancellationToken);
        await journal.SetWrittenAsync(user, day, written, now, cancellationToken);
        return new WritingDayChange(day, written, now);
    }

    public Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(UserId user, int limit = 50, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        return journal.GetHistoryAsync(user, limit, cancellationToken);
    }

    public async Task<IReadOnlyList<HistoryMonth>> GetHistoryByMonthAsync(UserId user, int limit = 10_000, CancellationToken cancellationToken = default)
    {
        var history = await GetHistoryAsync(user, limit, cancellationToken);

        // L'historique arrive déjà du plus récent au plus ancien : GroupBy conserve cet ordre.
        return history
            .GroupBy(c => (c.ChangedAt.Year, c.ChangedAt.Month))
            .Select(g => new HistoryMonth(g.Key.Year, g.Key.Month, g.ToList()))
            .ToList();
    }

    private static void EnsureNotInFuture(DateOnly day, DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.DateTime);
        if (day > today)
        {
            throw new WritingDayInFutureException(day, today);
        }
    }
}
