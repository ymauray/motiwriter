namespace MotiWriter.Core.Accounts;

public sealed class AccountService(IUserAccountStore store, TimeProvider timeProvider) : IAccountService
{
    /// <summary>Nom utilisé quand le fournisseur ne communique ni nom ni adresse (cas d'Apple, après la première connexion).</summary>
    public const string AnonymousName = "Plume anonyme";

    private const int MaxDisplayNameLength = 100;

    public async Task<UserAccount> SignInAsync(ExternalIdentity identity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentException.ThrowIfNullOrWhiteSpace(identity.Provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(identity.Subject);

        var name = Clean(identity.DisplayName);
        var email = Clean(identity.Email);

        var existing = await store.FindByLoginAsync(identity.Provider, identity.Subject, cancellationToken);
        if (existing is null)
        {
            return await store.CreateWithLoginAsync(
                name ?? NameFromEmail(email) ?? AnonymousName,
                email,
                identity.Provider,
                identity.Subject,
                timeProvider.GetLocalNow(),
                cancellationToken);
        }

        // On ne remplace jamais une information connue par une absence d'information.
        var updated = existing with
        {
            DisplayName = name ?? existing.DisplayName,
            Email = email ?? existing.Email,
        };
        if (updated != existing)
        {
            await store.UpdateProfileAsync(existing.Id, updated.DisplayName, updated.Email, cancellationToken);
        }
        return updated;
    }

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }
        return trimmed.Length <= MaxDisplayNameLength ? trimmed : trimmed[..MaxDisplayNameLength];
    }

    private static string? NameFromEmail(string? email)
    {
        var at = email?.IndexOf('@') ?? -1;
        return at > 0 ? email![..at] : null;
    }
}
