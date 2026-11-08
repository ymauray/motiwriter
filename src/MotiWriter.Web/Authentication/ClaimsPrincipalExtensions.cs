using System.Security.Claims;
using MotiWriter.Core.Accounts;

namespace MotiWriter.Web.Authentication;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Fournisseur utilisé pour la session en cours.</summary>
    public const string ProviderClaim = "motiwriter:provider";

    /// <summary>Identifiant MotiWriter de la personne connectée.</summary>
    /// <exception cref="InvalidOperationException">La personne n'est pas connectée.</exception>
    public static UserId GetUserId(this ClaimsPrincipal principal) =>
        UserId.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("Aucun compte MotiWriter n'est associé à cette session.");

    /// <summary>Construit l'identité de session à partir du compte MotiWriter.</summary>
    public static ClaimsPrincipal ToPrincipal(this UserAccount account, string provider, string authenticationType) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.DisplayName),
                new Claim(ProviderClaim, provider),
            ],
            authenticationType));

    /// <summary>
    /// Le nom le plus lisible communiqué par le fournisseur : nom d'affichage Twitch ou Discord,
    /// nom complet Google, prénom et nom Apple (première connexion uniquement).
    /// </summary>
    public static string? ExternalDisplayName(this ClaimsPrincipal principal)
    {
        foreach (var type in new[] { "urn:twitch:displayname", "urn:discord:global_name", ClaimTypes.Name })
        {
            var value = principal.FindFirstValue(type);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }
        var fullName = $"{principal.FindFirstValue(ClaimTypes.GivenName)} {principal.FindFirstValue(ClaimTypes.Surname)}".Trim();
        return fullName.Length > 0 ? fullName : null;
    }
}
