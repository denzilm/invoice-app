using FrontendMentor.InvoiceApp.Shared.Common;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Enums;

public sealed class PermissionStatusEnum : SmartEnum<PermissionStatusEnum>
{
    public static readonly PermissionStatusEnum Active = new(1, nameof(Active), "Active");
    public static readonly PermissionStatusEnum Inactive = new(2, nameof(Inactive), "Inactive");

    public PermissionStatusEnum(int value, string name, string displayName)
        : base(value, name, displayName) { }
}
