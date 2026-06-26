using FrontendMentor.InvoiceApp.Identity.Domain.Entities;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Tests.Entities;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Create_ShouldCreateValidRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string tokenHash = "token-hash";

        // Act
        var token = RefreshToken.Create(userId, tokenHash);

        // Assert
        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal(tokenHash, token.TokenHash);
        Assert.True(token.CreatedAt > DateTimeOffset.UtcNow.AddSeconds(-1) && token.CreatedAt < DateTimeOffset.UtcNow.AddSeconds(1));
        Assert.True(token.ExpiresAt > token.CreatedAt.AddDays(29) && token.ExpiresAt < token.CreatedAt.AddDays(31));
        Assert.Null(token.LastUsedAt);
        Assert.Null(token.RevokedAt);
        Assert.True(token.IsValid);
        Assert.False(token.IsExpired);
        Assert.False(token.IsRevoked);
        Assert.False(token.IsImpersonated);
        Assert.Null(token.ImpersonatedBy);
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var act = () => RefreshToken.Create(Guid.Empty, "hash");

        var ex = Assert.Throws<ArgumentException>(act);
        Assert.Equal("userId", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ShouldThrow_WhenTokenHashIsInvalid(string hash)
    {
        var act = () => RefreshToken.Create(Guid.NewGuid(), hash);

        var ex = Assert.Throws<ArgumentException>(act);
        Assert.Equal("tokenHash", ex.ParamName);
    }

    [Fact]
    public void CreateForImpersonation_ShouldCreateImpersonatedToken()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        var impersonatorId = Guid.NewGuid();

        // Act
        var token = RefreshToken
            .CreateForImpersonation(targetUserId, impersonatorId, "hash");

        // Assert
        Assert.Equal(targetUserId, token.UserId);
        Assert.Equal(impersonatorId, token.ImpersonatedBy);
        Assert.True(token.IsImpersonated);
        Assert.True(token.IsValid);
    }

    [Fact]
    public void CreateForImpersonation_ShouldThrow_WhenTargetUserIdIsEmpty()
    {
        var act = () => RefreshToken
            .CreateForImpersonation(Guid.Empty, Guid.NewGuid(), "hash");

        var ex = Assert.Throws<ArgumentException>(act);
        Assert.Equal("userId", ex.ParamName);
    }

    [Fact]
    public void CreateForImpersonation_ShouldThrow_WhenImpersonatorIdIsEmpty()
    {
        var act = () => RefreshToken
            .CreateForImpersonation(Guid.NewGuid(), Guid.Empty, "hash");

        var ex = Assert.Throws<ArgumentException>(act);
        Assert.Equal("impersonatorUserId", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateForImpersonation_ShouldThrow_WhenTokenHashIsInvalid(string hash)
    {
        var act = () => RefreshToken
            .CreateForImpersonation(Guid.NewGuid(), Guid.NewGuid(), hash);

        var ex = Assert.Throws<ArgumentException>(act);
        Assert.Equal("tokenHash", ex.ParamName);
    }

    [Fact]
    public void Rotate_ShouldUpdateTokenHashAndLastUsedAt()
    {
        // Arrange
        var token = RefreshToken.Create(Guid.NewGuid(), "old-hash");

        // Act
        token.Rotate("new-hash");

        // Assert
        Assert.Equal("new-hash", token.TokenHash);
        Assert.NotNull(token.LastUsedAt);
        Assert.True(token.LastUsedAt > DateTimeOffset.UtcNow.AddSeconds(-1) && token.LastUsedAt < DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Rotate_ShouldThrow_WhenTokenHashIsInvalid(string hash)
    {
        // Arrange
        var token = RefreshToken.Create(Guid.NewGuid(), "hash");

        // Act
        var act = () => token.Rotate(hash);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Revoke_ShouldRevokeToken()
    {
        // Arrange
        var token = RefreshToken.Create(Guid.NewGuid(), "hash");

        // Act
        token.Revoke();

        // Assert
        Assert.True(token.IsRevoked);
        Assert.False(token.IsValid);
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public void Revoke_ShouldBeIdempotent()
    {
        // Arrange
        var token = RefreshToken.Create(Guid.NewGuid(), "hash");

        token.Revoke();
        var revokedAt = token.RevokedAt;

        // Act
        token.Revoke();

        // Assert
        Assert.Equal(revokedAt, token.RevokedAt);
    }

    [Fact]
    public void IsImpersonated_ShouldBeFalse_ForNormalToken()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash");

        Assert.False(token.IsImpersonated);
    }

    [Fact]
    public void IsImpersonated_ShouldBeTrue_ForImpersonatedToken()
    {
        var token = RefreshToken
            .CreateForImpersonation(Guid.NewGuid(), Guid.NewGuid(), "hash");

        Assert.True(token.IsImpersonated);
    }
}
