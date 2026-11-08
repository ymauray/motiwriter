namespace MotiWriter.Core.Accounts;

/// <summary>Implémentation en mémoire, destinée aux tests.</summary>
public sealed class InMemoryUserAccountStore : IUserAccountStore
{
    private readonly Dictionary<UserId, UserAccount> _accounts = [];
    private readonly Dictionary<(string Provider, string Subject), UserId> _logins = [];
    private readonly Lock _lock = new();
    private long _nextId = 1;

    public Task<UserAccount?> FindByLoginAsync(string provider, string subject, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_logins.TryGetValue((provider, subject), out var id) ? _accounts[id] : null);
        }
    }

    public Task<UserAccount> CreateWithLoginAsync(
        string displayName, string? email, string provider, string subject, DateTimeOffset createdAt,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_logins.ContainsKey((provider, subject)))
            {
                throw new InvalidOperationException($"La connexion {provider}/{subject} est déjà associée à un compte.");
            }
            var account = new UserAccount(new UserId(_nextId++), displayName, email, createdAt);
            _accounts[account.Id] = account;
            _logins[(provider, subject)] = account.Id;
            return Task.FromResult(account);
        }
    }

    public Task UpdateProfileAsync(UserId id, string displayName, string? email, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _accounts[id] = _accounts[id] with { DisplayName = displayName, Email = email };
            return Task.CompletedTask;
        }
    }
}
