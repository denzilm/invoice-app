using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;
using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Handlers;

public sealed class V1TestHandler : INotificationHandler<TestMessageV1>
{
    public static readonly TaskCompletionSource<TestMessageV1> Tcs = new();

    public Task HandleAsync(TestMessageV1 notification, CancellationToken cancellationToken = default)
    {
        Tcs.TrySetResult(notification);

        return Task.CompletedTask;
    }
}
