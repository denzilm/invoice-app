using FrontendMentor.InvoiceApp.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence.Configurations;

public sealed class RefreshTokenEntityTypeConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens").HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id).ValueGeneratedNever().IsRequired();
        builder.Property(rt => rt.UserId).IsRequired();
        builder.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(512);
        builder.Property(rt => rt.CreatedAt).IsRequired();
        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.RevokedAt);
        builder.Property(rt => rt.LastUsedAt);
        builder.Property(rt => rt.ImpersonatedBy);

        builder.HasIndex(rt => rt.UserId, "IX_RefreshTokens_UserId");
        builder.HasIndex(rt => rt.TokenHash, "IX_RefreshTokens_TokenHash").IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rt => rt.ImpersonatedBy)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
