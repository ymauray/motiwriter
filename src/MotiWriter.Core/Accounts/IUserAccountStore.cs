namespace MotiWriter.Core.Accounts;

/// <summary>Stockage des comptes et des connexions externes qui y mènent.</summary>
public interface IUserAccountStore
{
    /// <summary>Retrouve le compte lié à une connexion externe, s'il existe.</summary>
    Task<UserAccount?> FindByLoginAsync(string provider, string subject, CancellationToken cancellationToken = default);

    /// <summary>Crée un compte et lui associe la connexion externe, en une seule opération.</summary>
    Task<UserAccount> CreateWithLoginAsync(
        string displayName, string? email, string provider, string subject, DateTimeOffset createdAt,
        CancellationToken cancellationToken = default);

    /// <summary>Met à jour le nom et l'adresse affichés.</summary>
    Task UpdateProfileAsync(UserId id, string displayName, string? email, CancellationToken cancellationToken = default);
}
