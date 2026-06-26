namespace FrontendMentor.InvoiceApp.Identity.Domain.Entities;

public sealed class RolePermission
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private RolePermission(Guid roleId, Guid permissionId)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; }
    public Guid PermissionId { get; }
    public Role Role { get; private set; }
    public Permission Permission { get; private set; }

    public override bool Equals(object? obj)
    {
        return
            obj is RolePermission other &&
            RoleId == other.RoleId &&
            PermissionId == other.PermissionId;
    }

    private bool Equals(RolePermission other)
    {
        return RoleId.Equals(other.RoleId) && PermissionId.Equals(other.PermissionId);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RoleId, PermissionId);
    }
}
