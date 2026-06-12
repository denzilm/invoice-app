using FrontendMentor.InvoiceApp.Identity.Domain.Enums;
using FrontendMentor.InvoiceApp.Shared.Domain;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Entities;

public sealed class Permission : EntityBase<Guid>
{
    private Permission(Guid id, string name, string description, PermissionStatusEnum status)
        : base(id)
    {
        Name = name;
        Description = description;
        Status = status;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public PermissionStatusEnum Status { get; private set; }
}
