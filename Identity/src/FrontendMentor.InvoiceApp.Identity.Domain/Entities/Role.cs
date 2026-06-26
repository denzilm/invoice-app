using FrontendMentor.InvoiceApp.Identity.Domain.Enums;
using FrontendMentor.InvoiceApp.Shared.Domain;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Entities;

public sealed class Role : EntityBase<Guid>
{
    public Role(Guid id, string name, string description, RoleStatusEnum status, bool isGlobal)
        : base(id)
    {
        Name = name;
        Description = description;
        Status = status;
        IsGlobal = isGlobal;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public RoleStatusEnum Status { get; private set; }
    public bool IsGlobal { get; private set; }

    private readonly IList<RolePermission> _rolePermissions = [];
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();
}
