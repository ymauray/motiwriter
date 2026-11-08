using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MotiWriter.Core.Accounts;

namespace MotiWriter.Web.Authentication;

/// <summary>
/// Points d'entrée de la connexion. Les formulaires sont en POST avec jeton anti-falsification
/// (validé automatiquement grâce aux paramètres <c>[FromForm]</c>).
/// </summary>
public static class AuthenticationEndpoints
{
    private const string ProviderItem = "motiwriter:provider";
    private const string DevelopmentProvider = "Development";

    public static IEndpointRouteBuilder MapMotiWriterAuthentication(this IEndpointRouteBuilder app)
    {
        // 1. Départ vers le fournisseur choisi.
        app.MapPost("/connexion/{provider}", (string provider, [FromForm] string? returnUrl, LoginProviders providers) =>
        {
            var login = providers.Find(provider);
            if (login is null)
            {
                return Results.Redirect("/connexion?erreur=fournisseur");
            }
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"/connexion/retour?returnUrl={Uri.EscapeDataString(SafeReturnUrl(returnUrl))}",
            };
            properties.Items[ProviderItem] = login.Scheme;
            return Results.Challenge(properties, [login.Scheme]);
        }).AllowAnonymous();

        // 2. Retour du fournisseur : on retrouve (ou crée) le compte, puis on ouvre notre propre session.
        app.MapGet("/connexion/retour", async (HttpContext http, string? returnUrl, IAccountService accounts) =>
        {
            var result = await http.AuthenticateAsync(AuthenticationSetup.ExternalScheme);
            await http.SignOutAsync(AuthenticationSetup.ExternalScheme);

            var subject = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!result.Succeeded
                || string.IsNullOrEmpty(subject)
                || result.Properties?.Items.TryGetValue(ProviderItem, out var provider) != true
                || string.IsNullOrEmpty(provider))
            {
                return Results.Redirect("/connexion?erreur=retour");
            }

            var account = await accounts.SignInAsync(new ExternalIdentity(
                provider, subject, result.Principal!.ExternalDisplayName(), result.Principal!.FindFirstValue(ClaimTypes.Email)));
            await SignInAsync(http, account, provider);
            return Results.LocalRedirect(SafeReturnUrl(returnUrl));
        }).AllowAnonymous();

        // Connexion de développement, sans fournisseur (désactivée par défaut, jamais hors Development).
        app.MapPost("/connexion/developpement", async (HttpContext http, [FromForm] string? returnUrl, LoginProviders providers, IAccountService accounts) =>
        {
            if (!providers.DevelopmentLoginEnabled)
            {
                return Results.NotFound();
            }
            var account = await accounts.SignInAsync(new ExternalIdentity(DevelopmentProvider, "developpement", "Compte de développement", null));
            await SignInAsync(http, account, DevelopmentProvider);
            return Results.LocalRedirect(SafeReturnUrl(returnUrl));
        }).AllowAnonymous();

        app.MapPost("/deconnexion", async (HttpContext http, [FromForm] string? returnUrl) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.LocalRedirect("/connexion");
        });

        return app;
    }

    private static Task SignInAsync(HttpContext http, UserAccount account, string provider) =>
        http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            account.ToPrincipal(provider, CookieAuthenticationDefaults.AuthenticationScheme),
            new AuthenticationProperties { IsPersistent = true });

    /// <summary>N'accepte qu'une adresse locale (« /historique »), jamais « //ailleurs » ni une URL absolue.</summary>
    public static string SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl)
        && returnUrl[0] == '/'
        && (returnUrl.Length == 1 || (returnUrl[1] != '/' && returnUrl[1] != '\\'))
        && !returnUrl.StartsWith("/connexion", StringComparison.OrdinalIgnoreCase)
            ? returnUrl
            : "/";
}
