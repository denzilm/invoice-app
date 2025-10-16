using FrontendMentor.InvoiceApp.Application.Abstractions.Repositories;
using FrontendMentor.InvoiceApp.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FrontendMentor.InvoiceApp.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationData(
        this IServiceCollection services, string connectionString, bool enableDevelopmentLogging)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            if (!enableDevelopmentLogging) return;

            // Configure logging for development purposes
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter((category, level) =>
                        category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information)
                    .AddConsole();
            });

            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.UseLoggerFactory(loggerFactory);

            options.ConfigureWarnings(warnings =>
            {
                warnings
                    .Log(
                        CoreEventId.CascadeDelete,
                        CoreEventId.FirstWithoutOrderByAndFilterWarning,
                        CoreEventId.RowLimitingOperationWithoutOrderByWarning);
            });
        });

        services.AddScoped<ICountryRepository, CountryRepository>();

        return services;
    }
}
