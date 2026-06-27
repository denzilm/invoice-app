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

    private readonly List<UserAccessGrant> _accessGrants = [];
    public IReadOnlyList<UserAccessGrant> AccessGrants => _accessGrants.AsReadOnly();

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

    public void AssignRole(Guid grantedByUserId, Guid? companyId, Role role)
    {
        if (grantedByUserId == Guid.Empty)
            throw new ArgumentException("Granted by user id cannot be empty.", nameof(grantedByUserId));

        if (_accessGrants.Any(ag => ag.RoleId == role.Id && ag.IsActive))
        {
            throw new InvalidOperationException("This user already has an active grant for the specified role.");
        }

        if (role.IsGlobal && companyId is not null)
        {
            throw new InvalidOperationException($"The role '{role.Name}' is global role and cannot be assigned to a company.");
        }

        var accessGrant = UserAccessGrant.Create(grantedByUserId, Id, companyId, role);
        _accessGrants.Add(accessGrant);
    }

    public void AssignPermission(Guid grantedByUserId, Guid? companyId, Permission permission)
    {
        if (grantedByUserId == Guid.Empty)
            throw new ArgumentException("Granted by user id cannot be empty.", nameof(grantedByUserId));

        var hasPermissionGrant = _accessGrants.Any(ag => ag.PermissionId == permission.Id && ag.IsActive);
        var hasPermissionGrantViaRole = _accessGrants
            .Where(ag => ag.RoleId is not null && ag.IsActive)
            .SelectMany(ag => ag.Role!.RolePermissions)
            .Any(rp => rp.PermissionId == permission.Id);

        if (hasPermissionGrant || hasPermissionGrantViaRole)
        {
            throw new InvalidOperationException("This user already has an active grant for the specified permission.");
        }

        var grant = UserAccessGrant.Create(grantedByUserId, Id, companyId, role: null, permission);
        _accessGrants.Add(grant);
    }

    public void RevokeRole(Guid roleId, Guid revokedByUserId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role id cannot be empty.", nameof(roleId));
        var accessGrant = _accessGrants.FirstOrDefault(ag => ag.RoleId == roleId && ag.IsActive);
        if (accessGrant is null)
            throw new InvalidOperationException("No active grant found for the specified role.");

        accessGrant.Revoke(revokedByUserId);
    }

    public void RevokePermission(Guid permissionId, Guid revokedByUserId)
    {
        if (permissionId == Guid.Empty)
            throw new ArgumentException("Permission id cannot be empty.", nameof(permissionId));
        if (revokedByUserId == Guid.Empty)
            throw new ArgumentException("Revoked by user id cannot be empty.", nameof(revokedByUserId));

        var accessGrant = _accessGrants.FirstOrDefault(ag => ag.PermissionId == permissionId && ag.IsActive);
        if (accessGrant is null)
            throw new InvalidOperationException("No active grant found for the specified permission.");

        accessGrant.Revoke(revokedByUserId);
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
