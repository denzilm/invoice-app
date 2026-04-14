using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;
using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Handlers;

public sealed class DeadLetterTestHandler : INotificationHandler<DeadLetterTestMessage>
{
    public static readonly TaskCompletionSource<bool> Tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task HandleAsync(DeadLetterTestMessage notification, CancellationToken cancellationToken = default)
    {
        throw new Exception("Always fails");
    }
}
