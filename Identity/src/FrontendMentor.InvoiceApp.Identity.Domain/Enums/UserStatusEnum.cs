using FrontendMentor.InvoiceApp.Shared.Common;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Enums;

public sealed class UserStatusEnum : SmartEnum<UserStatusEnum>
{
    public static readonly UserStatusEnum Active = new(1, nameof(Active), "Active");
    public static readonly UserStatusEnum Inactive = new(2, nameof(Inactive), "Inactive");

    private UserStatusEnum(int value, string name, string displayName)
        : base(value, name, displayName) { }
}
