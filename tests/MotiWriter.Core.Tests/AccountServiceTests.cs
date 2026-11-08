using Microsoft.Extensions.Time.Testing;
using MotiWriter.Core.Accounts;

namespace MotiWriter.Core.Tests;

public class AccountServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 18, 0, 0, TimeSpan.Zero);

    private readonly InMemoryUserAccountStore _store = new();
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _service = new AccountService(_store, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task First_sign_in_creates_the_account()
    {
        var account = await _service.SignInAsync(new ExternalIdentity("Discord", "42", "Yannick", null));

        Assert.Equal("Yannick", account.DisplayName);
        Assert.Equal(Now, account.CreatedAt);
        Assert.Equal(account, await _store.FindByLoginAsync("Discord", "42"));
    }

    [Fact]
    public async Task Next_sign_ins_return_the_same_account()
    {
        var first = await _service.SignInAsync(new ExternalIdentity("Google", "abc", "Yannick", null));
        var second = await _service.SignInAsync(new ExternalIdentity("Google", "abc", "Yannick", null));

        Assert.Equal(first.Id, second.Id);
    }

    [Fact]
    public async Task Same_subject_on_another_provider_is_another_account()
    {
        var discord = await _service.SignInAsync(new ExternalIdentity("Discord", "42", "A", null));
        var twitch = await _service.SignInAsync(new ExternalIdentity("Twitch", "42", "B", null));

        Assert.NotEqual(discord.Id, twitch.Id);
    }

    [Fact]
    public async Task Display_name_follows_the_provider_but_is_never_erased()
    {
        await _service.SignInAsync(new ExternalIdentity("Apple", "001", "Camille Martin", null));

        var renamed = await _service.SignInAsync(new ExternalIdentity("Apple", "001", "Camille M.", null));
        // Apple ne renvoie plus le nom après la première connexion.
        var silent = await _service.SignInAsync(new ExternalIdentity("Apple", "001", null, null));

        Assert.Equal("Camille M.", renamed.DisplayName);
        Assert.Equal("Camille M.", silent.DisplayName);
        Assert.Equal("Camille M.", (await _store.FindByLoginAsync("Apple", "001"))!.DisplayName);
    }

    [Theory]
    [InlineData(null, "camille@example.org", "camille")]
    [InlineData("  ", null, AccountService.AnonymousName)]
    public async Task Missing_name_falls_back_to_email_then_anonymous(string? name, string? email, string expected)
    {
        var account = await _service.SignInAsync(new ExternalIdentity("Apple", "002", name, email));

        Assert.Equal(expected, account.DisplayName);
    }

    [Fact]
    public async Task Provider_and_subject_are_required()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SignInAsync(new ExternalIdentity("", "1", "x", null)));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SignInAsync(new ExternalIdentity("Google", " ", "x", null)));
    }
}
