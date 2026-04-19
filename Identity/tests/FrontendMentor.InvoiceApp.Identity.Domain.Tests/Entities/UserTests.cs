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

    private static User CreateValidUser(string firstName = "Jane", string lastName = "Doe", string avatarUrl = ValidAvatarUrl) =>
        User.Create(firstName, lastName, Helpers.ValidEmail(), Helpers.ValidPhone(), avatarUrl);

    private static UserIdentity MakeIdentity(LoginProviderEnum provider, string key = "ac123") =>
        new(Guid.NewGuid(), provider, key);
}
