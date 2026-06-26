using FrontendMentor.InvoiceApp.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence.Configurations;

public sealed class RolePermissionEntityTypeConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions").HasKey(x => new { x.RoleId, x.PermissionId });
    }
}
