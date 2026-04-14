using System.Reflection;
using System.Runtime.CompilerServices;
using FrontendMentor.InvoiceApp.Shared.Notifications;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("FrontendMentor.InvoiceApp.Shared.Tests")]
[assembly: InternalsVisibleTo("FrontendMentor.InvoiceApp.Messaging.IntegrationTests")]

namespace FrontendMentor.InvoiceApp.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationExecutor(this IServiceCollection services, params Assembly[] assemblies)
    {
        var defaultAssemblies = new[] { Assembly.GetCallingAssembly(), Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly() }
            .Where(a => a is not null)
            .Cast<Assembly>()
            .ToArray();

        var assembliesToScan = assemblies.Length == 0 ? defaultAssemblies : assemblies;
        services.Scan(scan => scan.FromAssemblies(assembliesToScan)
            .AddClasses(classes => classes.AssignableTo(typeof(INotificationHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        services.AddSingleton<INotificationHandlerRegistry>(_ => new NotificationHandlerRegistry(assemblies));
        services.AddSingleton<INotificationRetryPolicy, NotificationRetryPolicy>();
        services.AddScoped<INotificationExecutor, NotificationExecutor>();

        return services;
    }
}
