namespace FrontendMentor.InvoiceApp.Shared.Notifications;

public enum NotificationExecutionStrategy
{
    /// <summary>
    /// Executes the notification handlers sequentially, one after the other.
    /// </summary>
    Sequential,

    /// <summary>
    /// Executes the notification handlers in parallel, allowing them to run concurrently.
    /// </summary>
    Parallel
}
