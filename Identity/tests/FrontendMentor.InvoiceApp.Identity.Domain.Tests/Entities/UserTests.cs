using FrontendMentor.InvoiceApp.Identity.Domain.Entities;
using FrontendMentor.InvoiceApp.Identity.Domain.Enums;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Tests.Entities;

public class UserTests
{
    private const string ValidAvatarUrl = "https://cdn.example.com/avatars/user.png";

    [Fact]
    public void Create_Returns_User_With_Correct_Properties()
    {
        var user = CreateValidUser();

        Assert.NotNull(user);
        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal(Helpers.ValidEmail(), user.EmailAddress);
        Assert.Equal(Helpers.ValidPhone(), user.PhoneNumber);
        Assert.Equal(ValidAvatarUrl, user.AvatarUrl);
    }

    [Fact]
    public void Trims_FirstName_Before_Storing()
    {
        var user = CreateValidUser(firstName: "  Jane  ");

        Assert.Equal("Jane", user.FirstName);
    }

    [Fact]
    public void Trims_LastName_Before_Storing()
    {
        var user = CreateValidUser(lastName: "  Doe  ");

        Assert.Equal("Doe", user.LastName);
    }

    [Fact]
    public void FullName_Combines_First_And_Last()
    {
        var user = CreateValidUser();

        Assert.Equal("Jane Doe", user.FullName);
    }

    [Fact]
    public void UserIdentities_Is_Empty_On_Creation()
    {
        var user = CreateValidUser();

        Assert.Empty(user.UserIdentities);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Throws_When_FirstName_Is_NullOrWhiteSpace(string? firstName)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => User.Create(firstName!, "Doe", Helpers.ValidEmail(), Helpers.ValidPhone(), ValidAvatarUrl));

        Assert.Equal("firstName", ex.ParamName);
    }

    [Fact]
    public void Throws_When_FirstName_Exceeds_100_Characters()
    {
        var longName = new string('A', 101);

        var ex = Assert.Throws<ArgumentException>(
            () => User.Create(longName, "Doe", Helpers.ValidEmail(), Helpers.ValidPhone(), ValidAvatarUrl));

        Assert.Equal("firstName", ex.ParamName);
    }

    [Fact]
    public void Accepts_FirstName_Of_Exactly_100_Characters()
    {
        var maxName = new string('A', 100);

        var user = User.Create(maxName, "Doe", Helpers.ValidEmail(), Helpers.ValidPhone(), ValidAvatarUrl);

        Assert.Equal(maxName, user.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Throws_When_LastName_Is_NullOrWhiteSpace(string? lastName)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => User.Create("Jane", lastName!, Helpers.ValidEmail(), Helpers.ValidPhone(), ValidAvatarUrl));

        Assert.Equal("lastName", ex.ParamName);
    }

    [Fact]
    public void Throws_When_LastName_Exceeds_100_Characters()
    {
        var longName = new string('B', 101);

        var ex = Assert.Throws<ArgumentException>(
            () => User.Create("Jane", longName, Helpers.ValidEmail(), Helpers.ValidPhone(), ValidAvatarUrl));

        // This also catches the nameof(firstName) bug — test will fail until fixed
        Assert.Equal("lastName", ex.ParamName);
    }

    [Fact]
    public void Accepts_LastName_Of_Exactly_100_Characters()
    {
        var maxName = new string('B', 100);

        var user = User.Create("Jane", maxName, Helpers.ValidEmail(), Helpers.ValidPhone(), ValidAvatarUrl);

        Assert.Equal(maxName, user.LastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Throws_When_AvatarUrl_Is_NullOrWhiteSpace(string? avatarUrl)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => User.Create("Jane", "Doe", Helpers.ValidEmail(), Helpers.ValidPhone(), avatarUrl!));

        Assert.Equal("avatarUrl", ex.ParamName);
    }

    [Fact]
    public void Adds_Identity_To_Collection()
    {
        var user = CreateValidUser();
        var identity = MakeIdentity(LoginProviderEnum.Local, "google-user-1");

        user.LinkIdentity(identity);

        Assert.Single(user.UserIdentities);
        Assert.Contains(identity, user.UserIdentities);
    }

    [Fact]
    public void Allows_Multiple_Distinct_Identities()
    {
        var user = CreateValidUser();

        user.LinkIdentity(MakeIdentity(LoginProviderEnum.Local, "g-123"));
        user.LinkIdentity(MakeIdentity(LoginProviderEnum.Local, "gh-456"));

        Assert.Equal(2, user.UserIdentities.Count);
    }

    [Fact]
    public void Allows_Same_Provider_With_Different_Key()
    {
        var user = CreateValidUser();

        user.LinkIdentity(MakeIdentity(LoginProviderEnum.Local, "key-1"));
        user.LinkIdentity(MakeIdentity(LoginProviderEnum.Local, "key-2"));

        Assert.Equal(2, user.UserIdentities.Count);
    }

    [Fact]
    public void Throws_When_Same_Provider_And_Key_Already_Linked()
    {
        var user = CreateValidUser();
        user.LinkIdentity(MakeIdentity(LoginProviderEnum.Local, "abc123"));

        var ex = Assert.Throws<InvalidOperationException>(
            () => user.LinkIdentity(MakeIdentity(LoginProviderEnum.Local, "abc123")));

        Assert.Contains("already linked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UserIdentities_Cannot_Be_Mutated_Externally()
    {
        var user = CreateValidUser();

        var collection = user.UserIdentities;

        Assert.IsType<IReadOnlyList<UserIdentity>>(collection, exactMatch: false);
        Assert.False(collection is List<UserIdentity>, "Collection should not be the raw list");
    }

    [Fact]
    public void TryGetValidRefreshToken_ShouldReturnTrue_WhenValidTokenExists()
    {
        var user = CreateValidUser();
        var token = RefreshToken.Create(user.Id, "hash");

        user.AddRefreshToken(token);

        var result = user.TryGetValidRefreshToken("hash", out var found);

        Assert.True(result);
        Assert.NotNull(found);
    }

    [Fact]
    public void TryGetValidRefreshToken_ShouldReturnFalse_WhenTokenDoesNotExist()
    {
        var user = CreateValidUser();

        var result = user.TryGetValidRefreshToken("missing", out var found);

        Assert.False(result);
        Assert.Null(found);
    }

    [Fact]
    public void TryGetValidRefreshToken_ShouldReturnFalse_WhenTokenIsRevoked()
    {
        var user = CreateValidUser();
        var token = RefreshToken.Create(user.Id, "hash");

        user.AddRefreshToken(token);
        user.RevokeRefreshToken("hash");

        var result = user.TryGetValidRefreshToken("hash", out var found);

        Assert.False(result);
        Assert.Null(found);
    }

    // -----------------------------
    // AddRefreshToken
    // -----------------------------

    [Fact]
    public void AddRefreshToken_ShouldAddToken()
    {
        var user = CreateValidUser();
        var token = RefreshToken.Create(user.Id, "hash");

        user.AddRefreshToken(token);

        Assert.True(user.TryGetValidRefreshToken("hash", out var found));
        Assert.NotNull(found);
    }

    [Fact]
    public void AddRefreshToken_ShouldThrow_WhenTokenBelongsToDifferentUser()
    {
        var user = CreateValidUser();
        var otherUserId = Guid.NewGuid();
        var token = RefreshToken.Create(otherUserId, "hash");

        var act = () => user.AddRefreshToken(token);

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("Refresh token does not belong to this user", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddRefreshToken_ShouldThrow_WhenTokenIsExpired()
    {
        var user = CreateValidUser();
        var token = RefreshToken.Create(user.Id, "hash");

        // force expire (hacky but common in unit tests unless you inject time)
        typeof(RefreshToken)
            .GetProperty("ExpiresAt")!
            .SetValue(token, DateTimeOffset.UtcNow.AddDays(-1));

        var act = () => user.AddRefreshToken(token);

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("Cannot add an expired refresh token.", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddRefreshToken_ShouldThrow_WhenDuplicateHashExists()
    {
        var user = CreateValidUser();

        var token1 = RefreshToken.Create(user.Id, "hash");
        var token2 = RefreshToken.Create(user.Id, "hash");

        user.AddRefreshToken(token1);

        var act = () => user.AddRefreshToken(token2);

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("A refresh token with the same hash already exists for this user.", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------
    // RotateRefreshToken
    // -----------------------------

    [Fact]
    public void RotateRefreshToken_ShouldRotateToken()
    {
        var user = CreateValidUser();
        var token = RefreshToken.Create(user.Id, "old");

        user.AddRefreshToken(token);

        user.RotateRefreshToken("old", "new");

        Assert.Equal("new", token.TokenHash);
    }

    [Fact]
    public void RotateRefreshToken_ShouldThrow_WhenTokenDoesNotExist()
    {
        var user = CreateValidUser();

        var act = () => user.RotateRefreshToken("missing", "new");

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("Refresh token is invalid or does not exist.", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------
    // RevokeRefreshToken
    // -----------------------------

    [Fact]
    public void RevokeRefreshToken_ShouldRevokeToken()
    {
        var user = CreateValidUser();
        var token = RefreshToken.Create(user.Id, "hash");

        user.AddRefreshToken(token);

        user.RevokeRefreshToken("hash");

        Assert.True(token.IsRevoked);
    }

    [Fact]
    public void RevokeRefreshToken_ShouldThrow_WhenTokenDoesNotExist()
    {
        var user = CreateValidUser();

        var act = () => user.RevokeRefreshToken("missing");

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("Refresh token not found.", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RevokeRefreshToken_ShouldThrow_WhenTokenAlreadyRevoked()
    {
        var user = CreateValidUser();
        var token = RefreshToken.Create(user.Id, "hash");

        user.AddRefreshToken(token);

        user.RevokeRefreshToken("hash");

        var act = () => user.RevokeRefreshToken("hash");

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("Refresh token is already revoked.", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------
    // RevokeAllRefreshTokens
    // -----------------------------

    [Fact]
    public void RevokeAllRefreshTokens_ShouldRevokeAllValidTokens()
    {
        var user = CreateValidUser();

        var t1 = RefreshToken.Create(user.Id, "h1");
        var t2 = RefreshToken.Create(user.Id, "h2");

        user.AddRefreshToken(t1);
        user.AddRefreshToken(t2);

        user.RevokeAllRefreshTokens();

        Assert.True(t1.IsRevoked);
        Assert.True(t2.IsRevoked);
    }

    // -----------------------------
    // RevokeAllRefreshTokensExcept
    // -----------------------------

    [Fact]
    public void RevokeAllRefreshTokensExcept_ShouldLeaveSpecifiedTokenValid()
    {
        var user = CreateValidUser();

        var keep = RefreshToken.Create(user.Id, "keep");
        var revoke = RefreshToken.Create(user.Id, "revoke");

        user.AddRefreshToken(keep);
        user.AddRefreshToken(revoke);

        user.RevokeAllRefreshTokensExcept("keep");

        Assert.False(keep.IsRevoked);
        Assert.True(revoke.IsRevoked);
    }

    [Fact]
    public void RevokeAllRefreshTokensExcept_ShouldRevokeAllOtherTokens()
    {
        var user = CreateValidUser();

        var keep = RefreshToken.Create(user.Id, "keep");
        var revoke1 = RefreshToken.Create(user.Id, "r1");
        var revoke2 = RefreshToken.Create(user.Id, "r2");

        user.AddRefreshToken(keep);
        user.AddRefreshToken(revoke1);
        user.AddRefreshToken(revoke2);

        user.RevokeAllRefreshTokensExcept("keep");

        Assert.True(revoke1.IsRevoked);
        Assert.True(revoke2.IsRevoked);
        Assert.False(keep.IsRevoked);
    }

    private static User CreateValidUser(string firstName = "Jane", string lastName = "Doe", string avatarUrl = ValidAvatarUrl) =>
        User.Create(firstName, lastName, Helpers.ValidEmail(), Helpers.ValidPhone(), avatarUrl);

    private static UserIdentity MakeIdentity(LoginProviderEnum provider, string key = "ac123") =>
        new(Guid.NewGuid(), provider, key);
}
