using FrontendMentor.InvoiceApp.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence.Configurations;

public sealed class UserRolePermissionEntityTypeConfiguration : IEntityTypeConfiguration<UserAccessGrant>
{
    public void Configure(EntityTypeBuilder<UserAccessGrant> builder)
    {
        builder.ToTable("UserRolePermissions").HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();

        builder.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}
