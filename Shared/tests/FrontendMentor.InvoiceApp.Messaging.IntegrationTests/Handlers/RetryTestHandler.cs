using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;
using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Handlers;

public sealed class RetryTestHandler : INotificationHandler<RetryMessage>
{
    private static int _attempts;

    public static readonly TaskCompletionSource<int> AttemptsTcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task HandleAsync(RetryMessage message, CancellationToken cancellationToken)
    {
        _attempts++;

        if (_attempts < 3)
            throw new Exception("fail");

        AttemptsTcs.TrySetResult(_attempts);
        return Task.CompletedTask;
    }
}
