using System.Reflection;
using FrontendMentor.InvoiceApp.Messaging.Abstractions;
using FrontendMentor.InvoiceApp.Messaging.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace FrontendMentor.InvoiceApp.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, MessagingOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("Messaging connection cannot be empty", nameof(options));

        if (options.MaxConcurrentCalls <= 0)
            throw new ArgumentOutOfRangeException(nameof(options.MaxConcurrentCalls), "Must be positive");

        var defaultAssemblies = new[] { Assembly.GetCallingAssembly(), Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly() }
            .Where(a => a is not null)
            .Cast<Assembly>()
            .ToArray();

        var assemblies = !options.Assemblies.Any() ? defaultAssemblies : options.Assemblies;

        services.AddSingleton<IMessageTopology, DefaultMessageTopology>();
        services.AddSingleton<IMessageRegistry>(_ => new MessageRegistry(assemblies));

        if (options.Provider == MessagingProvider.RabbitMq)
        {
            services.AddSingleton<RabbitMqConnectionProvider>(_ =>
                new RabbitMqConnectionProvider(options.ConnectionString));
            services.AddHostedService<RabbitMqMessageSubscriber>();
        }

        services.AddSingleton<IMessageBus>(sp =>
        {
            return options.Provider switch
            {
                MessagingProvider.RabbitMq => new RabbitMqMessageBus(
                    sp.GetRequiredService<RabbitMqConnectionProvider>(),
                    sp,
                    sp.GetRequiredService<IMessageTopology>(),
                    sp.GetRequiredService<IMessageRegistry>(),
                    options.MaxConcurrentCalls),
                _ => throw new InvalidOperationException("Invalid messaging provider")
            };
        });

        return services;
    }
}
