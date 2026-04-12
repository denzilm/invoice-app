using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Notifications;

public sealed class NoRetryNotificationPolicy : INotificationRetryPolicy
{
    public ValueTask ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        return new ValueTask(action(cancellationToken));
    }
}
