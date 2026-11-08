namespace MotiWriter.Core.Journal;

public sealed class WritingDayService(IWritingJournal journal, TimeProvider timeProvider) : IWritingDayService
{
    public async Task<WritingDayChange?> SetWrittenAsync(DateOnly day, bool written, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetLocalNow();
        EnsureNotInFuture(day, now);

        var changed = await journal.SetWrittenAsync(day, written, now, cancellationToken);
        return changed ? new WritingDayChange(day, written, now) : null;
    }

    public async Task<WritingDayChange> ToggleAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetLocalNow();
        EnsureNotInFuture(day, now);

        var written = !await journal.IsWrittenAsync(day, cancellationToken);
        await journal.SetWrittenAsync(day, written, now, cancellationToken);
        return new WritingDayChange(day, written, now);
    }

    public Task<IReadOnlyList<WritingDayChange>> GetHistoryAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        return journal.GetHistoryAsync(limit, cancellationToken);
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
