using System.Diagnostics;
using FrontendMentor.InvoiceApp.Identity.Infrastructure.IdentityPersistence;
using Microsoft.EntityFrameworkCore;

namespace FrontendMentor.InvoiceApp.Identity.MigrationService;

public sealed class MigrationHostedService : BackgroundService
{
    public const string ActivitySourceName = "IdentityMigrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public MigrationHostedService(IServiceProvider serviceProvider, IHostApplicationLifetime applicationLifetime)
    {
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

            await RunMigrationsAsync(dbContext, stoppingToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        _applicationLifetime.StopApplication();
    }

    private static async Task RunMigrationsAsync(AuthDbContext dbContext, CancellationToken stoppingToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(dbContext, static (context, ct) => context.Database.MigrateAsync(ct), stoppingToken);
    }
}
