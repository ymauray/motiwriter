using MotiWriter.Core.Accounts;
using MotiWriter.Core.Journal;
using MotiWriter.Core.Messages;
using MotiWriter.Core.Statistics;

namespace MotiWriter.Core.Dashboard;

public sealed class DashboardService(IWritingJournal journal, TimeProvider timeProvider) : IDashboardService
{
    public async Task<DashboardSummary> GetSummaryAsync(UserId user, int weeks = 20, int recentDays = 8, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weeks, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(recentDays, 1);

        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var days = await journal.GetWritingDaysAsync(user, cancellationToken);

        return new DashboardSummary(
            today,
            days.Contains(today),
            WritingStatisticsCalculator.Compute(days, today),
            BuildCalendar(days, today, weeks),
            BuildLastDays(days, today, recentDays),
            DailyMessages.For(today));
    }

    private static List<CalendarDay> BuildLastDays(IReadOnlySet<DateOnly> days, DateOnly today, int count) =>
        Enumerable.Range(0, count)
            .Select(i => today.AddDays(i - count + 1))
            .Select(date => new CalendarDay(date, days.Contains(date), IsFuture: false))
            .ToList();

    private static List<CalendarWeek> BuildCalendar(IReadOnlySet<DateOnly> days, DateOnly today, int weeks)
    {
        // Lundi de la semaine courante (DayOfWeek.Sunday = 0).
        var offsetFromMonday = ((int)today.DayOfWeek + 6) % 7;
        var start = today.AddDays(-offsetFromMonday - 7 * (weeks - 1));

        var result = new List<CalendarWeek>(weeks);
        for (var w = 0; w < weeks; w++)
        {
            var week = new List<CalendarDay>(7);
            for (var d = 0; d < 7; d++)
            {
                var date = start.AddDays(w * 7 + d);
                week.Add(new CalendarDay(date, days.Contains(date), date > today));
            }
            result.Add(new CalendarWeek(week));
        }
        return result;
    }
}
