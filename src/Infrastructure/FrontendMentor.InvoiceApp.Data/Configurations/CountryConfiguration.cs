using FrontendMentor.InvoiceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrontendMentor.InvoiceApp.Data.Configurations;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("newsequentialid()")
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(2)
            .IsFixedLength();

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasIndex(c => c.Name)
            .IsUnique();
    }
}
