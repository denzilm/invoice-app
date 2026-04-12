namespace FrontendMentor.InvoiceApp.Shared.Notifications;

public interface INotificationRetryPolicy
{
    ValueTask ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}
