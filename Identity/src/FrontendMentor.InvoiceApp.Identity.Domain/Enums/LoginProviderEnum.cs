using FrontendMentor.InvoiceApp.Shared.Common;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Enums;

public sealed class LoginProviderEnum : SmartEnum<LoginProviderEnum>
{
    public static readonly LoginProviderEnum Local = new(1, nameof(Local), "Local");

    private LoginProviderEnum(int value, string name, string displayName)
        : base(value, name, displayName) { }
}
