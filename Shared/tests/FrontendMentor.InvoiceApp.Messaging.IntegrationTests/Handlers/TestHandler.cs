using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;
using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Handlers;

public sealed class TestHandler : INotificationHandler<TestMessage>
{
    public static TaskCompletionSource<TestMessage> Tcs = new();

    public Task HandleAsync(TestMessage notification, CancellationToken cancellationToken = default)
    {
        Tcs.TrySetResult(notification);

        return Task.CompletedTask;
    }
}
