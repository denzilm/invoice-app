using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;
using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Handlers;

public sealed class ShutdownTestHandler : INotificationHandler<ShutdownTestMessage>
{
    public static TaskCompletionSource<bool> Started =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    public static TaskCompletionSource<bool> Completed =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public async Task HandleAsync(ShutdownTestMessage message, CancellationToken cancellationToken)
    {
        Started.TrySetResult(true);
        await Task.Delay(500, cancellationToken); // simulate slow work
        Completed.TrySetResult(true);
    }
}
