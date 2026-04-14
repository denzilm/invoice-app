using System.Reflection;
using FrontendMentor.InvoiceApp.Messaging.Abstractions;
using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Handlers;
using FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;
using FrontendMentor.InvoiceApp.Messaging.RabbitMQ;
using FrontendMentor.InvoiceApp.Shared;
using FrontendMentor.InvoiceApp.Shared.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests;

[Collection(nameof(RabbitMqCollection))]
public sealed class MessagingIntegrationTests
{
    private readonly RabbitMqFixture _rabbitMqFixture;

    public MessagingIntegrationTests(RabbitMqFixture rabbitMqFixture)
    {
        _rabbitMqFixture = rabbitMqFixture;
    }

    [Fact]
    public async Task Publish_Should_Be_Consumed_By_Handler()
    {
        TestHandler.Tcs = new TaskCompletionSource<TestMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var (publisher, _, _) = await SetupAsync();

        await publisher.PublishAsync(new TestMessage { Content = "hello world" });

        var message = await TestHandler.Tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal("hello world", message.Content);
        Assert.Equal(1, message.Version);
    }

    [Fact]
    public async Task Message_Should_Retry_Until_Success()
    {
        var (publisher, _, _) = await SetupAsync();

        await publisher.PublishAsync(new RetryMessage());

        var attempts = await RetryTestHandler.AttemptsTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(3, attempts); // 2 fails + 1 success
    }

    [Fact]
    public async Task Message_Should_Dead_Letter_After_Max_Retries()
    {
        var (publisher, provider, topology) = await SetupAsync();

        // Consume directly from the DLQ so we can observe when the message lands there
        var connection = await provider.GetRequiredService<RabbitMqConnectionProvider>().GetConnectionAsync();
        var dlqChannel = await connection.CreateChannelAsync();

        var consumerName = Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown";
        var dlqName = topology.GetDeadLetterQueueName(new DeadLetterTestMessage().Name, consumerName);

        var dlqConsumer = new AsyncEventingBasicConsumer(dlqChannel);
        dlqConsumer.ReceivedAsync += async (_, _) =>
        {
            DeadLetterTestHandler.Tcs.SetResult(true);
            await dlqChannel.BasicAckAsync(0, false);
        };
        await dlqChannel.BasicConsumeAsync(dlqName, autoAck: false, dlqConsumer);

        await publisher.PublishAsync(new DeadLetterTestMessage { Content = "Will fail" });
        var deadLettered = await DeadLetterTestHandler.Tcs.Task
            .WaitAsync(TimeSpan.FromMinutes(2)); // Allow for retry delays

        Assert.True(deadLettered);
    }

    [Fact]
    public async Task Should_Handle_Max_Concurrent_Messages()
    {
        const int maxConcurrentCalls = 5;

        ConcurrentTestHandler.Reset(maxConcurrentCalls);

        var (publisher, _, _) = await SetupAsync();

        var publishingTasks  = Enumerable.Range(0, maxConcurrentCalls)
            .Select(_ => publisher.PublishAsync(new ConcurrentTestMessage()));

        await Task.WhenAll(publishingTasks);

        var maxConcurrency = await ConcurrentTestHandler.AllHandled.Task
            .WaitAsync(TimeSpan.FromMinutes(2));

        Assert.InRange(maxConcurrency, 1, maxConcurrentCalls);
    }

    [Fact]
    public async Task Should_Route_Message_By_Version()
    {
        TestHandler.Tcs = new TaskCompletionSource<TestMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

        var (publisher, _, _) = await SetupAsync();

        await publisher.PublishAsync(new  TestMessageV1 { Content = "v1" });
        await publisher.PublishAsync(new  TestMessageV2 { Content = "v2", Extra = "extra" });

        var v1 = await V1TestHandler.Tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var v2 = await V2TestHandler.Tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal("v1", v1.Content);
        Assert.Equal("v2", v2.Content);
        Assert.Equal("extra", v2.Extra);
    }

    [Fact]
public async Task Should_Invoke_All_Handlers_For_Message()
{
    MultiHandlerA.Tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    MultiHandlerB.Tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    var (publisher, _, _) = await SetupAsync();

    await publisher.PublishAsync(new MultiHandlerTestMessage());

    await Task.WhenAll(
        MultiHandlerA.Tcs.Task.WaitAsync(TimeSpan.FromMinutes(1)),
        MultiHandlerB.Tcs.Task.WaitAsync(TimeSpan.FromMinutes(1)));
}

[Fact]
public async Task Publisher_Dispose_Should_Not_Affect_Consumer()
{
    TestHandler.Tcs = new TaskCompletionSource<TestMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

    var (publisher, _, _) = await SetupAsync();

    await publisher.PublishAsync(new TestMessage { Content = "after dispose" });
    await publisher.DisposeAsync(); // dispose before message is consumed

    var message = await TestHandler.Tcs.Task.WaitAsync(TimeSpan.FromMinutes(1));

    Assert.Equal("after dispose", message.Content);
}

[Fact]
public async Task Should_Complete_Inflight_Messages_Before_Shutdown()
{
    ShutdownTestHandler.Started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    ShutdownTestHandler.Completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    var (publisher, provider, _) = await SetupAsync();
    var hostedServices = provider.GetServices<IHostedService>().ToList();

    await publisher.PublishAsync(new ShutdownTestMessage());

    // Wait until the handler has started (i.e., is inflight)
    await ShutdownTestHandler.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));

    // Stop host while handler is still running
    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    foreach (var svc in hostedServices)
        await svc.StopAsync(cts.Token);

    // Handler should have completed despite shutdown
    var completed = await ShutdownTestHandler.Completed.Task
        .WaitAsync(TimeSpan.FromSeconds(5), cts.Token);

    Assert.True(completed);
}

    private async Task<(IMessagePublisher, ServiceProvider provider, IMessageTopology topology)> SetupAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        var assembly = Assembly.GetExecutingAssembly();
        services.AddMessaging(new MessagingOptions
        {
            ConnectionString = _rabbitMqFixture.ConnectionString,
            Provider = MessagingProvider.RabbitMq,
            MaxConcurrentCalls = 5,
            Assemblies = [assembly]
        });

        services.AddNotificationExecutor(assembly);
        services.AddSingleton<INotificationRetryPolicy, NotificationRetryPolicy>();

        var provider = services.BuildServiceProvider();
        foreach (var service in provider.GetServices<IHostedService>())
            await service.StartAsync(CancellationToken.None);

        var bus = provider.GetRequiredService<IMessageBus>();
        var publisher = await bus.CreatePublisherAsync();

        return (publisher, provider, provider.GetRequiredService<IMessageTopology>());
    }
}
