using FrontendMentor.InvoiceApp.Shared.Notifications;
using FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestNotifications;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestHandlers;

public abstract class AbstractHandler : INotificationHandler<TestNotification>
{
    public abstract Task HandleAsync(TestNotification notification, CancellationToken cancellationToken);
}
