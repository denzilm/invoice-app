using FrontendMentor.InvoiceApp.Shared.Domain;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Entities;

public sealed class UserAccessGrant : EntityBase<Guid>
{
    public UserAccessGrant(Guid id) : base(id) { }

    public Guid UserId { get; private set; }
    public Guid? CompanyId { get; private set; }

    public Guid GrantedByUserId { get; private set; }
    public Guid? RevokedByUserId { get; private set; }

    public DateTimeOffset? GrantedOn { get; private set; }
    public DateTimeOffset? RevokedOn { get; private set; }

    public Role? Role { get; private set; }
    public Guid? RoleId { get; private set; }

    public Permission? Permission { get; private set; }
    public Guid? PermissionId { get; private set; }

    public bool IsActive => RevokedOn is null && GrantedOn is not null;

    public static UserAccessGrant Create(
        Guid grantedByUserId, Guid assignedToUserId, Guid? companyId, Role? role, Permission? permission = null)
    {
        ValidateCreateRequest(grantedByUserId, assignedToUserId, companyId, role, permission);

        return new UserAccessGrant(Guid.CreateVersion7())
        {
            GrantedByUserId = grantedByUserId,
            UserId = assignedToUserId,
            CompanyId = companyId,

            Role = role,
            RoleId = role?.Id,

            Permission = permission,
            PermissionId = permission?.Id,

            GrantedOn = DateTimeOffset.UtcNow
        };
    }

    public void Revoke(Guid revokedByUserId)
    {
        if (revokedByUserId == Guid.Empty)
            throw new ArgumentException("Revoked by user id cannot be empty.", nameof(revokedByUserId));
        if (RevokedOn is not null)
            throw new InvalidOperationException("This role or permission has already been revoked.");

        RevokedByUserId = revokedByUserId;
        RevokedOn = DateTimeOffset.UtcNow;
    }

    private static void ValidateCreateRequest(
        Guid grantedByUserId, Guid assignedToUserId, Guid? companyId, Role? role, Permission? permission)
    {
        if (grantedByUserId == Guid.Empty)
            throw new ArgumentException("Granted by user cannot be empty.", nameof(grantedByUserId));
        if (assignedToUserId == Guid.Empty)
            throw new ArgumentException("Assigned to user cannot be empty.", nameof(assignedToUserId));
        if (role is null && permission is null)
            throw new ArgumentException("Either role or permission must be specified.");
        if (role is not null && permission is not null)
            throw new ArgumentException("Cannot assign both a role and a permission. Only one can be set");
        if (role is not null && role.IsGlobal && companyId is not null)
            throw new ArgumentException($"The role '{role.Name}' is global role and cannot be assigned to a company.");
    }
}
