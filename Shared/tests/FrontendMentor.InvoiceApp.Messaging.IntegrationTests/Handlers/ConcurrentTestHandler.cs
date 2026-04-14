using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;
using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Handlers;

public sealed class ConcurrentTestHandler : INotificationHandler<ConcurrentTestMessage>
{
    private static int _current;
    private static int _completed;
    private static int _expected;

    private static int _maxObservedConcurrency;
    public static TaskCompletionSource<int> AllHandled = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public static void Reset(int expected)
    {
        _current = 0;
        _completed = 0;
        _expected = expected;
        _maxObservedConcurrency = 0;
        AllHandled = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public async Task HandleAsync(ConcurrentTestMessage notification, CancellationToken cancellationToken = default)
    {
        var current = Interlocked.Increment(ref _current);
        int snapshot;
        do
        {
            snapshot = _maxObservedConcurrency;
        } while (snapshot < current &&
                 Interlocked.CompareExchange(ref _maxObservedConcurrency, current, snapshot) != snapshot);

        await Task.Delay(100, cancellationToken);

        Interlocked.Decrement(ref _current);
        if (Interlocked.Increment(ref _completed) == _expected)
            AllHandled.TrySetResult(_maxObservedConcurrency);
    }
}
