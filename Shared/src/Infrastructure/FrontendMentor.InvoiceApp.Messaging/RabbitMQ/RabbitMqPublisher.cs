using System.Text.Json;
using FrontendMentor.InvoiceApp.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FrontendMentor.InvoiceApp.Messaging.RabbitMQ;

public sealed class RabbitMqPublisher : IMessagePublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly IChannel _channel;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, IChannel channel)
    {
        _logger = logger;
        _channel = channel;
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        var exchange = message.Name;
        await _channel.ExchangeDeclareAsync(
            exchange, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: cancellationToken);

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Headers = new Dictionary<string, object?>
            {
                ["x-message-version"] = message.Version,
                ["x-delivery-attempt"] = 0
            }
        };

        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: string.Empty,
            mandatory: true,
            basicProperties: props,
            body: JsonSerializer.SerializeToUtf8Bytes(message),
            cancellationToken);

        _logger.LogDebug("Published message to {Exchange}", exchange);
    }

    public async ValueTask DisposeAsync() => await _channel.DisposeAsync();
}
