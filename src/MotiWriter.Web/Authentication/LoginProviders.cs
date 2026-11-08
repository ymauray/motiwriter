namespace MotiWriter.Web.Authentication;

/// <summary>Un fournisseur de connexion proposé sur la page de connexion.</summary>
/// <param name="Scheme">Nom du schéma d'authentification ASP.NET Core (et nom enregistré en base).</param>
/// <param name="DisplayName">Nom affiché sur le bouton.</param>
public sealed record LoginProvider(string Scheme, string DisplayName);

/// <summary>Les fournisseurs effectivement configurés, dans l'ordre d'affichage.</summary>
public sealed class LoginProviders(IReadOnlyList<LoginProvider> providers, bool developmentLogin)
{
    public IReadOnlyList<LoginProvider> All { get; } = providers;

    /// <summary>Connexion de développement sans fournisseur externe (jamais active hors environnement Development).</summary>
    public bool DevelopmentLoginEnabled { get; } = developmentLogin;

    public LoginProvider? Find(string scheme) =>
        All.FirstOrDefault(p => string.Equals(p.Scheme, scheme, StringComparison.OrdinalIgnoreCase));
}
