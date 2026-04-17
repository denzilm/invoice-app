using FrontendMentor.InvoiceApp.Identity.Domain.Enums;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Entities;

public sealed class UserIdentity
{
    public Guid UserId { get; }
    public LoginProviderEnum LoginProvider { get; }
    public string ProviderKey { get; }

    public UserIdentity(Guid userId, LoginProviderEnum loginProvider, string providerKey)
    {
        UserId = userId;
        LoginProvider = loginProvider;
        ProviderKey = providerKey;
    }
}
