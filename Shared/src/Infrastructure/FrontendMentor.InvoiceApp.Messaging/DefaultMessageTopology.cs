using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging;

internal class DefaultMessageTopology : IMessageTopology
{
    private const int DefaultMaxDeliveryCount = 5;

    public int MaxDeliveryCount => DefaultMaxDeliveryCount;

    public string GetQueueName(string messageName, string consumer) => $"{messageName}.{consumer}";

    public string GetRetryQueueName(string messageName, string consumer) => $"{messageName}.{consumer}.retry";

    public string GetDeadLetterQueueName(string messageName, string consumer) => $"{messageName}.{consumer}.dlq";

    public TimeSpan GetRetryDelay(int attempt)
    {
        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
        return baseDelay + jitter;
    }
}
