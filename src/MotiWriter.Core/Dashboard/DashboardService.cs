using MotiWriter.Core.Journal;
using MotiWriter.Core.Statistics;

namespace MotiWriter.Core.Dashboard;

public sealed class DashboardService(IWritingJournal journal, TimeProvider timeProvider) : IDashboardService
{
    public async Task<DashboardSummary> GetSummaryAsync(int weeks = 20, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weeks, 1);

        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var days = await journal.GetWritingDaysAsync(cancellationToken);

        return new DashboardSummary(
            today,
            WritingStatisticsCalculator.Compute(days, today),
            BuildCalendar(days, today, weeks));
    }

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
