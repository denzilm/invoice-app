using Microsoft.EntityFrameworkCore;

namespace FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence;

public sealed class IdentityAppDbContext : DbContext
{
    public IdentityAppDbContext(DbContextOptions<IdentityAppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityAppDbContext).Assembly, type => type.Namespace != null && type.Namespace.Contains("AppPersistence.Configurations"));
    }
}
