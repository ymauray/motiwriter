using System.Globalization;
using MotiWriter.Core.Dashboard;
using MotiWriter.Core.Journal;

namespace MotiWriter.Web.Formatting;

/// <summary>Formulations françaises des dates et des modifications, pour l'interface.</summary>
public static class FrenchDates
{
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");

    /// <summary>« mardi 3 novembre », « samedi 1er novembre », l'année n'étant précisée que si elle diffère.</summary>
    public static string Day(DateOnly day, DateOnly today)
    {
        var dayNumber = day.Day == 1 ? "1er" : day.Day.ToString(French);
        var label = $"{day.ToString("dddd", French)} {dayNumber} {day.ToString("MMMM", French)}";
        return day.Year == today.Year ? label : $"{label} {day.Year}";
    }

    /// <summary>« 19 h 42 ».</summary>
    public static string Time(DateTimeOffset moment) => moment.ToString("H' h 'mm", French);

    /// <summary>« Le mardi 3 novembre a été marqué écrit le dimanche 8 novembre à 19 h 42. »</summary>
    public static string Describe(WritingDayChange change, DateOnly today)
    {
        var changedOn = DateOnly.FromDateTime(change.ChangedAt.DateTime);
        var state = change.Written ? "écrit" : "non écrit";
        return $"Le {Day(change.Day, today)} a été marqué {state} le {Day(changedOn, today)} à {Time(change.ChangedAt)}.";
    }

    /// <summary>Libellé accessible d'une case cliquable du calendrier.</summary>
    public static string ToggleLabel(CalendarDay day, DateOnly today)
    {
        var label = day.Date == today ? $"Aujourd'hui, {Day(day.Date, today)}" : Capitalize(Day(day.Date, today));
        if (day.IsFuture)
        {
            return $"{label} — à venir";
        }
        return day.Written
            ? $"{label} — écrit · cliquer pour décocher"
            : $"{label} — non écrit · cliquer pour marquer écrit";
    }

    /// <summary>« 1 jour », « 97 jours ».</summary>
    public static string Days(int count) => count > 1 ? $"{count} jours" : $"{count} jour";

    private static string Capitalize(string text) =>
        string.IsNullOrEmpty(text) ? text : char.ToUpper(text[0], French) + text[1..];
}
