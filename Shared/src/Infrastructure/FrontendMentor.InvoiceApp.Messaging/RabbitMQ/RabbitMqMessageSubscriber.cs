using System.Reflection;
using FrontendMentor.InvoiceApp.Messaging.Abstractions;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace FrontendMentor.InvoiceApp.Messaging.RabbitMQ;

public sealed class RabbitMqMessageSubscriber : IHostedService
{
    private readonly List<IMessageHandler> _handlers = [];

    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly IMessageBus _messageBus;
    private readonly IMessageRegistry _messageRegistry;
    private readonly IMessageTopology _messageTopology;

    public RabbitMqMessageSubscriber(
        RabbitMqConnectionProvider connectionProvider,
        IMessageBus messageBus,
        IMessageRegistry messageRegistry,
        IMessageTopology messageTopology)
    {
        _connectionProvider = connectionProvider;
        _messageBus = messageBus;
        _messageRegistry = messageRegistry;
        _messageTopology = messageTopology;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var consumerName = Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";

        foreach (var descriptor in _messageRegistry.GetRegisteredTypes())
        {
            var exchangeName = descriptor.Name;
            var queueName = _messageTopology.GetQueueName(exchangeName, consumerName);
            var retryQueueName = _messageTopology.GetRetryQueueName(exchangeName, consumerName);
            var deadLetterQueueName = _messageTopology.GetDeadLetterQueueName(exchangeName, consumerName);

            await channel.ExchangeDeclareAsync(
                exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(
                deadLetterQueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queueName, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = string.Empty,
                    ["x-dead-letter-routing-key"] = deadLetterQueueName,
                },cancellationToken: cancellationToken);
            await channel.QueueBindAsync(queueName, exchangeName, string.Empty, cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                retryQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = string.Empty,
                    ["x-dead-letter-routing-key"] = queueName,
                }, cancellationToken: cancellationToken);

            var handler = await _messageBus.CreateListenerAsync(consumerName, cancellationToken);
            await handler.StartAsync(descriptor, cancellationToken);
            _handlers.Add(handler);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Dispose each handler
        foreach (var handler in _handlers)
            await handler.DisposeAsync();
    }
}
