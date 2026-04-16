using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using FrontendMentor.InvoiceApp.Messaging.Abstractions;
using FrontendMentor.InvoiceApp.Shared.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FrontendMentor.InvoiceApp.Messaging.RabbitMQ;

public sealed class RabbitMqMessageHandler : IMessageHandler
{
    private static readonly ConcurrentDictionary<Type, Func<INotificationExecutor, IMessage, CancellationToken, Task>>
        Dispatchers = new();
    private Channel<BasicDeliverEventArgs> _pipe = null!;
    private IReadOnlyList<Task> _workers = [];

    private string _consumerTag = null!;

    private readonly ILogger<RabbitMqMessageHandler> _logger;
    private readonly IChannel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageRegistry _messageRegistry;
    private readonly IMessageTopology _messageTopology;
    private readonly string _consumer;
    private readonly int _maxConcurrency;
    private readonly CancellationTokenSource _drainCts = new();

    public RabbitMqMessageHandler(
        ILogger<RabbitMqMessageHandler> logger,
        string consumer,
        IChannel channel,
        IServiceProvider serviceProvider,
        IMessageRegistry messageRegistry,
        IMessageTopology messageTopology,
        int maxConcurrency)
    {
        _logger = logger;
        _channel = channel;
        _serviceProvider = serviceProvider;
        _messageRegistry = messageRegistry;
        _messageTopology = messageTopology;
        _consumer = consumer;
        _maxConcurrency = maxConcurrency;
    }

    public async Task StartAsync(MessageDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        var queueName = _messageTopology.GetQueueName(descriptor.Name, _consumer);
        _logger.LogInformation("Starting RabbitMQ listener for queue {QueueName}", queueName);

        _pipe = Channel.CreateBounded<BasicDeliverEventArgs>(
            new BoundedChannelOptions(_maxConcurrency)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = true
            });

        _workers = Enumerable
            .Range(0, _maxConcurrency)
            .Select(_ => Task.Run(() => ProcessLoopAsync(descriptor, cancellationToken), CancellationToken.None))
            .ToList();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            // CancellationToken.None - we control shutdown via BasicCancelAsync
            // not by abandoning mid-write
            await _pipe.Writer.WriteAsync(eventArgs, CancellationToken.None);
        };

        await _channel.BasicQosAsync(0, (ushort)_maxConcurrency, false, cancellationToken);

        // Store tag so we can cancel the consumer on shutdown
        _consumerTag = await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer, cancellationToken);
    }

    private async Task ProcessLoopAsync(MessageDescriptor descriptor, CancellationToken cancellationToken)
    {
        await foreach (var eventArgs in _pipe.Reader.ReadAllAsync(_drainCts.Token))
        {
            var version = GetHeaderValue(eventArgs.BasicProperties.Headers, "x-message-version", defaultValue: 1);
            var type = _messageRegistry.Resolve(descriptor.Name, version);
            try
            {
                var message = JsonSerializer.Deserialize(eventArgs.Body.ToArray(), type) as IMessage;
                await HandleMessageAsync(message, type, cancellationToken);
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message of type {MessageType}",
                    $"{descriptor.Name} v{version}");

                var retryCount = GetHeaderValue(eventArgs.BasicProperties.Headers, "x-delivery-attempt", defaultValue: 0);
                if (retryCount < _messageTopology.MaxDeliveryCount)
                {
                    await RepublishWithDelay(eventArgs, descriptor, retryCount + 1, CancellationToken.None);
                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, CancellationToken.None);
                }
                else
                {
                    await _channel.BasicRejectAsync(eventArgs.DeliveryTag, false, CancellationToken.None);
                    _logger.LogError(
                        "Message processing failed after {Attempts} attempts. Sending to dead-letter queue",
                        retryCount);
                }
            }
        }
    }

    private async Task HandleMessageAsync(IMessage? message, Type concreteType, CancellationToken cancellationToken)
    {
        if (message is null) return;

        using var scope = _serviceProvider.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<INotificationExecutor>();

        var dispatch = Dispatchers.GetOrAdd(concreteType, BuildDispatcher);
        await dispatch(executor, message, cancellationToken);
    }

    private static Func<INotificationExecutor, IMessage, CancellationToken, Task> BuildDispatcher(Type messageType)
    {
       var executorParam = Expression.Parameter(typeof(INotificationExecutor), "executor");
       var messageParam = Expression.Parameter(typeof(IMessage), "message");
       var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

       var method = typeof(INotificationExecutor)
           .GetMethod(nameof(INotificationExecutor.ExecuteAsync))!
           .MakeGenericMethod(messageType);

       var call =  Expression.Call(
           executorParam,
           method,
           Expression.Convert(messageParam, messageType),
           Expression.Constant(NotificationExecutionStrategy.Parallel), ctParam);

       return Expression
           .Lambda<Func<INotificationExecutor, IMessage, CancellationToken, Task>>(call, executorParam, messageParam, ctParam)
           .Compile();
    }

    private async Task RepublishWithDelay(
        BasicDeliverEventArgs eventArgs, MessageDescriptor descriptor, int nextRetry, CancellationToken cancellationToken)
    {
        var delay = _messageTopology.GetRetryDelay(nextRetry);
        var props = new BasicProperties(eventArgs.BasicProperties)
        {
            Headers = eventArgs.BasicProperties.Headers ?? new Dictionary<string, object>()!,
        };
        props.Headers["x-delivery-attempt"] = nextRetry;
        props.Expiration = delay.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _messageTopology.GetRetryQueueName(descriptor.Name, _consumer),
            mandatory: true,
            basicProperties: props,
            body: eventArgs.Body,
            cancellationToken);
    }

    private static int GetHeaderValue(IDictionary<string, object?>? headers, string key, int defaultValue)
    {
        if (headers is null || !headers.TryGetValue(key, out var value) || value is null)
            return defaultValue;

        return value switch
        {
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
            int i => i,
            long l and >= int.MinValue and <= int.MaxValue => (int)l,
            _ => defaultValue
        };
    }

    public async ValueTask DisposeAsync()
    {
        // Tell the broker to stop pushing new messages
        await _channel.BasicCancelAsync(_consumerTag);

        // Stop the RabbitMQ consumer pushing new messages into the pipe
        _pipe.Writer.Complete();

        // Give workers time to drain what's already in the channel
        if (_workers.Count > 0)
            await Task.WhenAll(_workers);

        // Now signal the drain token - workers have already exited ReadAllAsync naturally
        await _drainCts.CancelAsync();
        _drainCts.Dispose();

        await _channel.DisposeAsync();
    }
}
