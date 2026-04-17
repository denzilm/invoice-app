using FrontendMentor.InvoiceApp.Shared.Common;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Tests;

public static class Helpers
{
    public static EmailAddress ValidEmail() => EmailAddress.Create("test@example.com");
    public static PhoneNumber ValidPhone() => PhoneNumber.Create("+27219047314");
}
