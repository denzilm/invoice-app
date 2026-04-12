namespace FrontendMentor.InvoiceApp.Shared.Notifications;

public interface INotificationExecutor
{
    Task ExecuteAsync<T>(
        T notification,
        NotificationExecutionStrategy strategy = NotificationExecutionStrategy.Parallel,
        CancellationToken cancellationToken = default) where T : INotification;
}
