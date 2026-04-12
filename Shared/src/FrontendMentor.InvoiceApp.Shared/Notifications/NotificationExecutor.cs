using Microsoft.Extensions.DependencyInjection;

namespace FrontendMentor.InvoiceApp.Shared.Notifications;

internal sealed class NotificationExecutor : INotificationExecutor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly INotificationHandlerRegistry _handlerRegistry;
    private readonly INotificationRetryPolicy _retryPolicy;

    public NotificationExecutor(
        IServiceScopeFactory serviceScopeFactory,
        INotificationHandlerRegistry handlerRegistry,
        INotificationRetryPolicy retryPolicy)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _handlerRegistry = handlerRegistry;
        _retryPolicy = retryPolicy;
    }

    public async Task ExecuteAsync<T>(
        T notification,
        NotificationExecutionStrategy strategy = NotificationExecutionStrategy.Parallel,
        CancellationToken cancellationToken = default) where T : INotification
    {
        var handlers = _handlerRegistry.GetHandlersForNotification(typeof(T));

        if (handlers.Count == 0) return;

        switch (strategy)
        {
            case NotificationExecutionStrategy.Parallel:
                await Parallel.ForEachAsync(handlers, cancellationToken, async (handlerType, ct) =>
                {
                    await ExecuteHandler(notification, handlerType, ct);
                });
                break;
            case NotificationExecutionStrategy.Sequential:
            {
                foreach (var handlerType in handlers)
                {
                    await ExecuteHandler(notification, handlerType, cancellationToken);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
        }
    }

    private async Task ExecuteHandler<T>(T notification, Type handlerType, CancellationToken ct) where T : INotification
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = (INotificationHandler<T>)scope.ServiceProvider.GetRequiredService(handlerType);
        await _retryPolicy.ExecuteAsync(token => handler.HandleAsync(notification, token), ct);
    }
}
