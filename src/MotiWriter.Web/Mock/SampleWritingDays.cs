namespace MotiWriter.Web.Mock;

/// <summary>Données fictives pour la maquette : ~5 mois d'écriture irrégulière.</summary>
public static class SampleWritingDays
{
    public static IEnumerable<DateOnly> Generate(DateOnly today)
    {
        var random = new Random(42); // déterministe
        for (var offset = 150; offset >= 1; offset--)
        {
            // Une série récente de 6 jours, et ~55 % des autres jours.
            if (offset <= 6 || (offset > 7 && random.NextDouble() < 0.55))
            {
                yield return today.AddDays(-offset);
            }
        }
    }
}
