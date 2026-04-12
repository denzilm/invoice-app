using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace FrontendMentor.InvoiceApp.Shared.Notifications;

public sealed class NotificationRetryPolicy : INotificationRetryPolicy
{
    private readonly ResiliencePipeline _retryPipeline;

    public NotificationRetryPolicy(ILogger<NotificationRetryPolicy> logger)
    {
        var options = new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,

            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromMilliseconds(200),

            UseJitter = true,

            ShouldHandle = new PredicateBuilder()
                .Handle<TimeoutException>()
                .Handle<HttpRequestException>()
                .Handle<Exception>(IsTransient),

            OnRetry = args =>
            {
                logger.LogInformation(
                    "Retry {AttemptNumber} after {RetryDelay}. Error: {Message}", args.AttemptNumber, args.RetryDelay, args.Outcome.Exception?.Message);
                return default;
            }
        };

        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(options)
            .Build();
    }

    public ValueTask ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        return _retryPipeline
            .ExecuteAsync(static (state, ct) => new ValueTask(state(ct)), action, cancellationToken);
    }

    private static bool IsTransient(Exception ex)
    {
        return
            ex is TimeoutException ||
            ex is HttpRequestException ||
            ex.GetType().Name.Contains("Transient", StringComparison.OrdinalIgnoreCase);
    }
}
