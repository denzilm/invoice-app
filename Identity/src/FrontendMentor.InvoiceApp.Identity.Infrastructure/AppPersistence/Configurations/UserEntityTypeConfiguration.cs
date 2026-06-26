using FrontendMentor.InvoiceApp.Identity.Domain.Entities;
using FrontendMentor.InvoiceApp.Identity.Domain.Enums;
using FrontendMentor.InvoiceApp.Shared.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence.Configurations;

public sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users").HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.EmailAddress).HasMaxLength(256).HasConversion(x => x.Value, x => EmailAddress.Create(x)).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(50).HasConversion(x => x.Number, x => PhoneNumber.Create(x)).IsRequired();
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Status).HasColumnName("StatusId").HasConversion(x => x.Value, x => UserStatusEnum.FromValue(x)).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.UserIdentities).WithOne()
            .HasForeignKey(u => u.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasIndex(["EmailAddress"], "IX_Users_EmailAddress").IsUnique();
        builder.HasIndex(["Status"], "IX_Users_Status");
    }
}
