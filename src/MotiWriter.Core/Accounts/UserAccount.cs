namespace MotiWriter.Core.Accounts;

/// <summary>Un compte MotiWriter. Il n'y a ni mot de passe ni code : on se connecte uniquement via un fournisseur OAuth.</summary>
/// <param name="Id">Identifiant interne.</param>
/// <param name="DisplayName">Nom affiché, repris du fournisseur.</param>
/// <param name="Email">Adresse fournie par le fournisseur, si elle l'a communiquée (indicative, jamais utilisée pour identifier).</param>
/// <param name="CreatedAt">Date de création du compte.</param>
public sealed record UserAccount(UserId Id, string DisplayName, string? Email, DateTimeOffset CreatedAt);

/// <summary>L'identité renvoyée par un fournisseur OAuth après connexion.</summary>
/// <param name="Provider">Nom technique du fournisseur (« Google », « Discord », « Twitch », « Apple »).</param>
/// <param name="Subject">Identifiant stable de la personne chez ce fournisseur.</param>
/// <param name="DisplayName">Nom communiqué par le fournisseur, s'il y en a un.</param>
/// <param name="Email">Adresse communiquée par le fournisseur, s'il y en a une.</param>
public sealed record ExternalIdentity(string Provider, string Subject, string? DisplayName, string? Email);
