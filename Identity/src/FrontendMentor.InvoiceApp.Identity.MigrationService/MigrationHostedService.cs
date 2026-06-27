using System.Diagnostics;
using FrontendMentor.InvoiceApp.Identity.Domain.Entities;
using FrontendMentor.InvoiceApp.Identity.Domain.Enums;
using FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence;
using FrontendMentor.InvoiceApp.Identity.Infrastructure.IdentityPersistence;
using FrontendMentor.InvoiceApp.Shared.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FrontendMentor.InvoiceApp.Identity.MigrationService;

public sealed class MigrationHostedService : BackgroundService
{
    public const string ActivitySourceName = "IdentityMigrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private readonly ILogger<MigrationHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public MigrationHostedService(
        ILogger<MigrationHostedService> logger,
        IServiceProvider serviceProvider,
        IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Client);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var appDbContext = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();

            await RunMigrationsAsync(dbContext, stoppingToken);
            await RunMigrationsAsync(appDbContext, stoppingToken);
            await SeedDatabaseAsync(scope, stoppingToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        _applicationLifetime.StopApplication();
    }

    private static async Task RunMigrationsAsync(DbContext dbContext, CancellationToken stoppingToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(dbContext, static (context, ct) => context.Database.MigrateAsync(ct), stoppingToken);
    }

    private async Task SeedDatabaseAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();
        if (applicationDbContext.Set<User>().Any())
        {
            _logger.LogInformation("Database already seeded. Skipping seed operation");
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string superAdminEmail = "superAdmin@simple-invoicing.com";
        const string superAdminPassword = "P@ssw0rd";
        const string superAdminPhone = "+1 231 232 2334";
        const string superAdminFirstName = "Super";
        const string superAdminLastName = "Admin";

        var existingAppUser = await userManager.FindByEmailAsync(superAdminEmail);
        if (existingAppUser is not null)
        {
            _logger.LogInformation("Super admin user already exists. Skipping seed operation");
            return;
        }

        var superAdmin = new ApplicationUser
        {
            Email = superAdminEmail,
            UserName = superAdminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(superAdmin, superAdminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
            _logger.LogError("Failed to create super admin user: {Errors}", errors);
            throw new InvalidOperationException($"Failed to create super admin user: {errors}");
        }

        var strategy = applicationDbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(applicationDbContext, async (_, ct) =>
        {
            await using var transaction = await applicationDbContext.Database.BeginTransactionAsync(ct);
            try
            {
                var email = EmailAddress.Create(superAdmin.Email);
                var phone = PhoneNumber.Create(superAdminPhone);
                const string avatarUrl = "https://ui-avatars.com/api/?name=Super+Admin";

                var user = User.Create(superAdminFirstName, superAdminLastName, email, phone, avatarUrl);
                user.LinkIdentity(new UserIdentity(user.Id, LoginProviderEnum.Local, superAdmin.Id.ToString()));

                applicationDbContext.Set<User>().Add(user);
                await applicationDbContext.SaveChangesAsync(ct);

                var adminRole = await applicationDbContext.Set<Role>()
                    .Where(role => role.Name == "SuperUser")
                    .FirstOrDefaultAsync(ct);

                if (adminRole is null)
                {
                    _logger.LogWarning("Super user role not found. User created without role assignment");
                }
                else
                {
                    user.AssignRole(user.Id, companyId: null, adminRole);
                    await applicationDbContext.SaveChangesAsync(ct);
                }

                await transaction.CommitAsync(ct);
                _logger.LogInformation("Super admin user created successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error occurred while seeding database. Transaction rolled back");

                // Clean up the ApplicationUser if domain user creation failed
                try
                {
                    var createdUser = await userManager.FindByEmailAsync(superAdminEmail);
                    if (createdUser != null)
                    {
                        await userManager.DeleteAsync(createdUser);
                        _logger.LogInformation("Cleaned up ApplicationUser after failed seed operation");
                    }
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Failed to clean up ApplicationUser after seed failure");
                }

                throw;
            }
        }, cancellationToken);
    }
}
