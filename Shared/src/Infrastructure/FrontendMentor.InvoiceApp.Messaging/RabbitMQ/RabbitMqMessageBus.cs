using FrontendMentor.InvoiceApp.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FrontendMentor.InvoiceApp.Messaging.RabbitMQ;

public sealed class RabbitMqMessageBus : IMessageBus
{
    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageTopology _messageTopology;
    private readonly IMessageRegistry _messageRegistry;
    private readonly int _maxConcurrentCalls;

    public RabbitMqMessageBus(
        RabbitMqConnectionProvider connectionProvider,
        IServiceProvider serviceProvider,
        IMessageTopology messageTopology,
        IMessageRegistry messageRegistry,
        int maxConcurrentCalls)
    {
        _connectionProvider = connectionProvider;
        _serviceProvider = serviceProvider;
        _messageTopology = messageTopology;
        _messageRegistry = messageRegistry;
        _maxConcurrentCalls = maxConcurrentCalls;
    }

    public async Task<IMessagePublisher> CreatePublisherAsync(CancellationToken cancellationToken = default)
    {
        var connection = await _connectionProvider.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        var logger = _serviceProvider.GetRequiredService<ILogger<RabbitMqPublisher>>();

        return new RabbitMqPublisher(logger, channel);
    }

    public async Task<IMessageHandler> CreateListenerAsync(string consumer, CancellationToken cancellationToken = default)
    {
        var connection = await _connectionProvider.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        var logger = _serviceProvider.GetRequiredService<ILogger<RabbitMqMessageHandler>>();

        return new RabbitMqMessageHandler(
            logger, consumer, channel, _serviceProvider, _messageRegistry, _messageTopology, _maxConcurrentCalls);
    }

    public ValueTask DisposeAsync() => _connectionProvider.DisposeAsync();
}
