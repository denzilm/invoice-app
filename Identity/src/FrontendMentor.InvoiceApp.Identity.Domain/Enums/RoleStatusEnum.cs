using FrontendMentor.InvoiceApp.Shared.Common;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Enums;

public sealed class RoleStatusEnum : SmartEnum<RoleStatusEnum>
{
    public static readonly RoleStatusEnum Active = new(1, nameof(Active), "Active");
    public static readonly RoleStatusEnum Inactive = new(2, nameof(Inactive), "Inactive");
    public RoleStatusEnum(int value, string name, string displayName)
        : base(value, name, displayName) { }
}
