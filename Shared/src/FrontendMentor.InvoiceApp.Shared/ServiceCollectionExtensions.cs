using System.Reflection;
using System.Runtime.CompilerServices;
using FrontendMentor.InvoiceApp.Shared.Notifications;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("FrontendMentor.InvoiceApp.Shared.Tests")]

namespace FrontendMentor.InvoiceApp.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationExecutor(this IServiceCollection services, params Assembly[] assemblies)
    {

        services.Scan(scan => scan.FromAssembliesOf(typeof(INotificationHandler<>))
            .AddClasses(classes => classes.AssignableTo(typeof(INotificationHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.AddSingleton<INotificationHandlerRegistry>(_ => new NotificationHandlerRegistry(assemblies));
        services.AddSingleton<INotificationRetryPolicy, NotificationRetryPolicy>();
        services.AddScoped<INotificationExecutor, NotificationExecutor>();

        return services;
    }
}
