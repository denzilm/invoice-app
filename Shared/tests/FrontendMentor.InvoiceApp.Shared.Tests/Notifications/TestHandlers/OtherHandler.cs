using FrontendMentor.InvoiceApp.Shared.Notifications;
using FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestNotifications;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestHandlers;

public class OtherHandler : INotificationHandler<OtherNotification>
{
    public Task HandleAsync(OtherNotification notification, CancellationToken cancellationToken) => Task.CompletedTask;
}
