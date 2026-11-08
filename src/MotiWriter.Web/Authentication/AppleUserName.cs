using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace MotiWriter.Web.Authentication;

/// <summary>
/// Apple n'inscrit pas le nom dans le jeton : il l'envoie à part, dans le champ de formulaire
/// <c>user</c> (<c>{"name":{"firstName":"…","lastName":"…"}}</c>), et seulement lors de la toute
/// première autorisation. Le gestionnaire Apple ne le lit pas : on le transforme ici en claims
/// <see cref="ClaimTypes.GivenName"/> et <see cref="ClaimTypes.Surname"/>.
/// </summary>
internal static class AppleUserName
{
    public static async Task AddClaimsAsync(OAuthCreatingTicketContext context)
    {
        if (context.Identity is not { } identity || !context.Request.HasFormContentType)
        {
            return;
        }

        var form = await context.Request.ReadFormAsync(context.HttpContext.RequestAborted);
        var json = form["user"].ToString();
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            using var user = JsonDocument.Parse(json);
            if (!user.RootElement.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.Object)
            {
                return;
            }
            Add(identity, ClaimTypes.GivenName, name, "firstName");
            Add(identity, ClaimTypes.Surname, name, "lastName");
        }
        catch (JsonException)
        {
            // Champ mal formé : on se passe du nom plutôt que de refuser la connexion.
        }
    }

    private static void Add(ClaimsIdentity identity, string claimType, JsonElement name, string property)
    {
        if (identity.HasClaim(c => c.Type == claimType)
            || !name.TryGetProperty(property, out var value)
            || value.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(value.GetString()))
        {
            return;
        }
        identity.AddClaim(new Claim(claimType, value.GetString()!.Trim(), ClaimValueTypes.String, Issuer));
    }

    private const string Issuer = "Apple";
}
