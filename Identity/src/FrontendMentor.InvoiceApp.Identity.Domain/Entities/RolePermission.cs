namespace FrontendMentor.InvoiceApp.Identity.Domain.Entities;

public sealed class RolePermission
{
    private RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; }
    public Guid PermissionId { get; }
}
