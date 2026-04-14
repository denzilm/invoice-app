using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;
using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Handlers;

public sealed class V2TestHandler : INotificationHandler<TestMessageV2>
{
    public static readonly TaskCompletionSource<TestMessageV2> Tcs = new();

    public Task HandleAsync(TestMessageV2 notification, CancellationToken cancellationToken = default)
    {
        Tcs.TrySetResult(notification);

        return Task.CompletedTask;
    }
}
