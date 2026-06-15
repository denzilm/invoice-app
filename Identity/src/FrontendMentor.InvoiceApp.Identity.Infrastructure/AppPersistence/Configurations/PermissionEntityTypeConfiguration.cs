using FrontendMentor.InvoiceApp.Identity.Domain.Entities;
using FrontendMentor.InvoiceApp.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence.Configurations;

public sealed class PermissionEntityTypeConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions").HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd().HasDefaultValueSql("newsequentialid()");
        builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Description).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Status).HasColumnName("StatusId")
            .HasConversion(s => s.Value, v => PermissionStatusEnum.FromValue(v))
            .IsRequired();

        builder.HasIndex(p => p.Name, "UQ_Permissions_Name").IsUnique();

        builder.HasMany(p => p.RolePermissions).WithOne()
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
