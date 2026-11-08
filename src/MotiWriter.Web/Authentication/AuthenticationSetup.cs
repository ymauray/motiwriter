using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.Discord;
using AspNet.Security.OAuth.Twitch;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace MotiWriter.Web.Authentication;

/// <summary>
/// Authentification par cookie, alimentée uniquement par des fournisseurs OAuth :
/// MotiWriter ne stocke ni mot de passe ni code à usage unique.
/// </summary>
/// <remarks>
/// Chaque fournisseur n'est activé que si ses identifiants sont présents dans la configuration
/// (section <c>Authentication</c>, à renseigner via <c>dotnet user-secrets</c> en développement).
/// On ne demande que le strict nécessaire : un identifiant stable et un nom, jamais l'adresse e-mail.
/// </remarks>
public static class AuthenticationSetup
{
    /// <summary>Cookie temporaire qui porte l'identité externe entre le retour du fournisseur et notre propre connexion.</summary>
    public const string ExternalScheme = "MotiWriter.External";

    public static WebApplicationBuilder AddMotiWriterAuthentication(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection("Authentication");
        var secureCookies = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        var providers = new List<LoginProvider>();

        var auth = builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "MotiWriter";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = secureCookies;
                options.LoginPath = "/connexion";
                options.AccessDeniedPath = "/connexion";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
            })
            .AddCookie(ExternalScheme, options =>
            {
                options.Cookie.Name = "MotiWriter.External";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = secureCookies;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            });

        if (TryGetClient(section, "Discord", out var discord))
        {
            auth.AddDiscord(options =>
            {
                options.SignInScheme = ExternalScheme;
                (options.ClientId, options.ClientSecret) = discord;
                options.Scope.Clear();
                options.Scope.Add("identify");
            });
            providers.Add(new LoginProvider(DiscordAuthenticationDefaults.AuthenticationScheme, "Discord"));
        }

        if (TryGetClient(section, "Twitch", out var twitch))
        {
            auth.AddTwitch(options =>
            {
                options.SignInScheme = ExternalScheme;
                (options.ClientId, options.ClientSecret) = twitch;
                options.Scope.Clear(); // pas d'e-mail : l'identifiant et le nom suffisent
            });
            providers.Add(new LoginProvider(TwitchAuthenticationDefaults.AuthenticationScheme, "Twitch"));
        }

        if (TryGetClient(section, "Google", out var google))
        {
            auth.AddGoogle(options =>
            {
                options.SignInScheme = ExternalScheme;
                (options.ClientId, options.ClientSecret) = google;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
            });
            providers.Add(new LoginProvider(GoogleDefaults.AuthenticationScheme, "Google"));
        }

        var apple = section.GetSection("Apple");
        // La clé .p8 se donne soit en texte (PrivateKey), soit par le chemin du fichier (PrivateKeyPath),
        // plus pratique sur un serveur : une variable d'environnement sur plusieurs lignes passe mal.
        var privateKey = ReadApplePrivateKey(apple, builder.Environment.ContentRootPath);
        if (HasValues(apple, "ClientId", "TeamId", "KeyId") && privateKey is not null)
        {
            auth.AddApple(options =>
            {
                options.SignInScheme = ExternalScheme;
                options.ClientId = apple["ClientId"]!;
                options.TeamId = apple["TeamId"]!;
                options.KeyId = apple["KeyId"]!;
                options.GenerateClientSecret = true;
                options.PrivateKey = (_, _) => Task.FromResult(privateKey.AsMemory());
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("name"); // Apple ne transmet le nom qu'à la toute première connexion
                options.Events.OnCreatingTicket = AppleUserName.AddClaimsAsync;
            });
            providers.Add(new LoginProvider(AppleAuthenticationDefaults.AuthenticationScheme, "Apple"));
        }

        // Connexion de développement : uniquement en environnement Development ET si elle est activée explicitement.
        var developmentLogin = builder.Environment.IsDevelopment()
                               && section.GetValue<bool>("Development:Enabled");

        builder.Services.AddSingleton(new LoginProviders(providers, developmentLogin));
        builder.Services.AddAuthorization();
        builder.Services.AddCascadingAuthenticationState();
        return builder;
    }

    private static bool TryGetClient(IConfigurationSection section, string name, out (string ClientId, string ClientSecret) client)
    {
        var provider = section.GetSection(name);
        client = (provider["ClientId"] ?? "", provider["ClientSecret"] ?? "");
        return HasValues(provider, "ClientId", "ClientSecret");
    }

    private static string? ReadApplePrivateKey(IConfigurationSection apple, string contentRoot)
    {
        if (!string.IsNullOrWhiteSpace(apple["PrivateKey"]))
        {
            return apple["PrivateKey"];
        }
        var path = apple["PrivateKeyPath"];
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }
        // Fichier absent ou illisible : erreur au démarrage plutôt qu'à la première connexion.
        return File.ReadAllText(Path.GetFullPath(path, contentRoot));
    }

    private static bool HasValues(IConfigurationSection section, params string[] keys) =>
        keys.All(key => !string.IsNullOrWhiteSpace(section[key]));
}
