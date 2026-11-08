namespace MotiWriter.Core.Accounts;

public interface IAccountService
{
    /// <summary>
    /// Retrouve le compte associé à une identité externe, ou le crée à la première connexion.
    /// Le nom et l'adresse sont rafraîchis quand le fournisseur les communique.
    /// </summary>
    Task<UserAccount> SignInAsync(ExternalIdentity identity, CancellationToken cancellationToken = default);
}
