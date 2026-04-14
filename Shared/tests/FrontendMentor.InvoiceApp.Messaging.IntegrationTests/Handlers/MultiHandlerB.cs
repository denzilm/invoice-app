using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;
using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Handlers;

public sealed class MultiHandlerB : INotificationHandler<MultiHandlerTestMessage>
{
    public static TaskCompletionSource<bool> Tcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task HandleAsync(MultiHandlerTestMessage message, CancellationToken cancellationToken)
    {
        Tcs.TrySetResult(true);
        return Task.CompletedTask;
    }
}
