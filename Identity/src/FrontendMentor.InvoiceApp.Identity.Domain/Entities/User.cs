using FrontendMentor.InvoiceApp.Identity.Domain.Enums;
using FrontendMentor.InvoiceApp.Shared.Common;
using FrontendMentor.InvoiceApp.Shared.Domain;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Entities;

public sealed class User : EntityBase<Guid>
{
    private User(
        Guid id, string firstName, string lastName, EmailAddress emailAddress, PhoneNumber phoneNumber,
        string avatarUrl, UserStatusEnum status, DateTimeOffset createdAt) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        PhoneNumber = phoneNumber;
        AvatarUrl = avatarUrl;
        Status = status;
        CreatedAt = createdAt;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public string AvatarUrl { get; private set; }
    public UserStatusEnum Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<UserIdentity> _userIdentities = [];
    public IReadOnlyList<UserIdentity> UserIdentities => _userIdentities.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}";

    public static User Create(
        string firstName, string lastName, EmailAddress emailAddress, PhoneNumber phoneNumber, string avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));

        var trimmedFirstName = firstName.Trim();
        if (trimmedFirstName.Length > 100)
            throw new ArgumentException("First name exceeds maximum length", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        var trimmedLastName = lastName.Trim();
        if (trimmedLastName.Length > 100)
            throw new ArgumentException("Last name exceeds maximum length", nameof(lastName));

        if (!Uri.TryCreate(avatarUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Avatar URL must be a valid absolute URI", nameof(avatarUrl));

        return new User(Guid.CreateVersion7(), trimmedFirstName, trimmedLastName, emailAddress, phoneNumber, avatarUrl, UserStatusEnum.Active, DateTimeOffset.UtcNow);
    }

    public void LinkIdentity(UserIdentity identity)
    {
        if (_userIdentities.Any(ui =>
                ui.LoginProvider == identity.LoginProvider && ui.ProviderKey == identity.ProviderKey))
        {
            throw new InvalidOperationException("This identity is already linked to this user");
        }

        _userIdentities.Add(identity);
    }

    public bool TryGetValidRefreshToken(string tokenHash, out RefreshToken? refreshToken)
    {
        refreshToken = _refreshTokens.FirstOrDefault(rt => rt.TokenHash == tokenHash && rt.IsValid);
        return refreshToken is not null;
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        if (refreshToken.UserId != Id)
            throw new InvalidOperationException("Refresh token does not belong to this user");
        if (refreshToken.IsExpired)
            throw new InvalidOperationException("Cannot add an expired refresh token.");
        if (_refreshTokens.Any(rt => rt.TokenHash == refreshToken.TokenHash))
            throw new InvalidOperationException("A refresh token with the same hash already exists for this user.");

        _refreshTokens.Add(refreshToken);
    }

    public void RotateRefreshToken(string currentHash, string newHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currentHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(newHash);

        if (_refreshTokens.Any(rt => rt.TokenHash == newHash))
            throw new InvalidOperationException(
                "A refresh token with the same hash already exists for this user.");

        if (!TryGetValidRefreshToken(currentHash, out var refreshToken))
            throw new InvalidOperationException(
                "Refresh token is invalid or does not exist.");

        refreshToken!.Rotate(newHash);
    }

    public void RevokeRefreshToken(string tokenHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.TokenHash == tokenHash);
        if (refreshToken is null)
            throw new InvalidOperationException("Refresh token not found.");

        if (refreshToken.IsRevoked)
            throw new InvalidOperationException("Refresh token is already revoked.");

        refreshToken.Revoke();
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(rt => rt.IsValid))
            token.Revoke();
    }

    public void RevokeAllRefreshTokensExcept(string tokenHash)
    {
        foreach (var token in _refreshTokens.Where(rt => rt.IsValid && rt.TokenHash != tokenHash))
            token.Revoke();
    }
}
