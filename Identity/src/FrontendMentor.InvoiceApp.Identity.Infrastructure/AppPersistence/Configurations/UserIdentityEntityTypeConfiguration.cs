using FrontendMentor.InvoiceApp.Identity.Domain.Entities;
using FrontendMentor.InvoiceApp.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence.Configurations;

public sealed class UserIdentityEntityTypeConfiguration : IEntityTypeConfiguration<UserIdentity>
{
    public void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        builder.ToTable("UserIdentities").HasKey(x => new { x.LoginProvider, x.ProviderKey });
        builder.Property(x => x.LoginProvider).HasMaxLength(128).HasConversion(x => x.Value, x => LoginProviderEnum.FromValue(x)).IsRequired();
        builder.Property(x => x.ProviderKey).HasMaxLength(256).IsRequired();

        builder.HasIndex(["LoginProvider", "ProviderKey"], "IX_UserIdentities_LoginProvider_ProviderKey").IsUnique();
    }
}
