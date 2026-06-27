using FrontendMentor.InvoiceApp.Shared.Domain;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Entities;

public sealed class RefreshToken : EntityBase<Guid>
{
    private const int RefreshTokenLifetimeInDays = 30;

    private RefreshToken(Guid id, string tokenHash)
        : base(id)
    {
        TokenHash = tokenHash;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
    public Guid? ImpersonatedBy { get; private set; }

    public static RefreshToken Create(Guid userId, string tokenHash)
    {
        return CreateInternal(userId, tokenHash, impersonatedBy: null);
    }

    public static RefreshToken CreateForImpersonation(Guid targetUserId, Guid impersonatorUserId, string tokenHash)
    {
        if (impersonatorUserId == Guid.Empty)
            throw new ArgumentException("Impersonator user id cannot be empty.", nameof(impersonatorUserId));

        return CreateInternal(targetUserId, tokenHash, impersonatedBy: impersonatorUserId);
    }

    private static RefreshToken CreateInternal(Guid userId, string tokenHash, Guid? impersonatedBy)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty.", nameof(userId));

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        var now = DateTimeOffset.UtcNow;

        return new RefreshToken(Guid.CreateVersion7(), tokenHash)
        {
            UserId = userId,
            CreatedAt = now,
            ExpiresAt = now.AddDays(RefreshTokenLifetimeInDays),
            ImpersonatedBy = impersonatedBy
        };
    }

    public void Rotate(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash cannot be empty.", nameof(tokenHash));

        TokenHash = tokenHash;
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        RevokedAt ??= DateTimeOffset.UtcNow;
    }

    public bool IsImpersonated => ImpersonatedBy is not null;
    public bool IsExpired => ExpiresAt < DateTimeOffset.UtcNow;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsValid => !IsExpired && !IsRevoked;
}
