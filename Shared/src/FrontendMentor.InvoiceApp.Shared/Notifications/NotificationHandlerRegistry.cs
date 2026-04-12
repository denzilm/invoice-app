using System.Collections.Concurrent;
using System.Reflection;

namespace FrontendMentor.InvoiceApp.Shared.Notifications;

public sealed class NotificationHandlerRegistry : INotificationHandlerRegistry
{
    private readonly IReadOnlyList<Assembly> _assemblies;
    private readonly ConcurrentDictionary<Type, IReadOnlyList<Type>> _handlersCache = new();

    public NotificationHandlerRegistry(IReadOnlyList<Assembly> assemblies)
    {
        _assemblies = assemblies;
    }

    public IReadOnlyList<Type> GetHandlersForNotification(Type notificationType)
    {
        if (!typeof(INotification).IsAssignableFrom(notificationType))
        {
            throw new ArgumentException($"Type {notificationType.FullName} does not implement INotification.", nameof(notificationType));
        }

        return _handlersCache.GetOrAdd(notificationType, ResolveHanders);
    }

    private IReadOnlyList<Type> ResolveHanders(Type notificationType)
    {
        return _assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == typeof(INotificationHandler<>) &&
                          i.GetGenericArguments()[0] == notificationType))
            .ToList();
    }
}
