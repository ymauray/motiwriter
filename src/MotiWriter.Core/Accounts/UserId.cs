using System.Globalization;

namespace MotiWriter.Core.Accounts;

/// <summary>Identifiant interne d'un compte MotiWriter.</summary>
public readonly record struct UserId(long Value)
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public static bool TryParse(string? value, out UserId id)
    {
        var ok = long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var raw) && raw > 0;
        id = ok ? new UserId(raw) : default;
        return ok;
    }
}
