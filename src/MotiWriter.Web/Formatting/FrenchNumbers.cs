namespace MotiWriter.Web.Formatting;

/// <summary>Nombres écrits en toutes lettres, pour les titres.</summary>
public static class FrenchNumbers
{
    private static readonly string[] Units =
    [
        "zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf",
        "dix", "onze", "douze", "treize", "quatorze", "quinze", "seize",
        "dix-sept", "dix-huit", "dix-neuf",
    ];

    private static readonly string[] Tens = ["", "", "vingt", "trente", "quarante", "cinquante", "soixante"];

    /// <summary>De 0 à 69, en lettres (orthographe traditionnelle) ; au-delà, en chiffres.</summary>
    public static string InWords(int n)
    {
        if (n is < 0 or >= 70)
        {
            return n.ToString();
        }
        if (n < 20)
        {
            return Units[n];
        }
        var (ten, unit) = (n / 10, n % 10);
        return unit switch
        {
            0 => Tens[ten],
            1 => $"{Tens[ten]} et un",
            _ => $"{Tens[ten]}-{Units[unit]}",
        };
    }

    /// <summary>« Aujourd'hui », « Les huit derniers jours ».</summary>
    public static string LastDaysTitle(int days) =>
        days <= 1 ? "Aujourd'hui" : $"Les {InWords(days)} derniers jours";

    /// <summary>« La dernière semaine », « Les quarante-deux dernières semaines ».</summary>
    public static string LastWeeksTitle(int weeks) =>
        weeks <= 1 ? "La dernière semaine" : $"Les {InWords(weeks)} dernières semaines";
}
