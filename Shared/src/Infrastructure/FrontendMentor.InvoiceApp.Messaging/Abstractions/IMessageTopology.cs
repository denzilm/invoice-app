namespace FrontendMentor.InvoiceApp.Messaging.Abstractions;

public interface IMessageTopology
{
    string GetQueueName(string messageName, string consumer);
    string GetRetryQueueName(string messageName, string consumer);
    string GetDeadLetterQueueName(string messageName, string consumer);
    int MaxDeliveryCount { get; }
    TimeSpan GetRetryDelay(int attempt);
}
