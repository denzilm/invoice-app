using FrontendMentor.InvoiceApp.Shared.Notifications;
using FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestNotifications;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestHandlers;

public class TestHandler : INotificationHandler<TestNotification>
{
    public int CallCount;
    public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref CallCount);
        return Task.CompletedTask;
    }
}

public class SecondHandler : INotificationHandler<TestNotification>
{
    public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken) => Task.CompletedTask;
}
