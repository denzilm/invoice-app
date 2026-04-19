using FrontendMentor.InvoiceApp.Shared.Notifications;
using FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestHandlers;
using FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestNotifications;
using Microsoft.Extensions.DependencyInjection;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Notifications;

public sealed class NotificationExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldDoNothing_WhenNoHandlers()
    {
        SetupDependencies(out var scope, out var provider, out var registry, out var scopeFactory);

        registry.GetHandlersForNotification(typeof(TestNotification)).Returns([]);
        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);

        var executor = CreateExecutor(scopeFactory, registry);

        await executor.ExecuteAsync(new TestNotification());

        // No exception = success
    }

    [Fact]
    public async Task ExecuteAsync_Sequential_ShouldCallAllHandlers()
    {
        SetupDependencies(out var scope, out var provider, out var registry, out var scopeFactory);

        var handler1 = new TestHandler();
        var handler2 = new TestHandler();

        registry.GetHandlersForNotification(typeof(TestNotification)).Returns([typeof(TestHandler), typeof(TestHandler)]);
        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);
        provider.GetService(typeof(TestHandler)).Returns(handler1, handler2);

        var executor = CreateExecutor(scopeFactory, registry);

        await executor.ExecuteAsync(new TestNotification(), NotificationExecutionStrategy.Sequential);

        Assert.Equal(2, handler1.CallCount + handler2.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_Parallel_ShouldCallAllHandlers()
    {
        SetupDependencies(out var scope, out var provider, out var registry, out var scopeFactory);

        var handler1 = new TestHandler();
        var handler2 = new TestHandler();

        registry.GetHandlersForNotification(typeof(TestNotification)).Returns([typeof(TestHandler), typeof(TestHandler)]);
        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);
        provider.GetService(typeof(TestHandler)).Returns(handler1, handler2);

        var executor = CreateExecutor(scopeFactory, registry);

        // ReSharper disable once RedundantArgumentDefaultValue
        await executor.ExecuteAsync(new TestNotification(), NotificationExecutionStrategy.Parallel);

        Assert.Equal(2, handler1.CallCount + handler2.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateScopePerHandler()
    {
        SetupDependencies(out var scope, out var provider, out var registry, out var scopeFactory);

        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);

        var handler = new TestHandler();
        provider.GetService(typeof(TestHandler)).Returns(handler);

        registry.GetHandlersForNotification(typeof(TestNotification))
            .Returns([typeof(TestHandler), typeof(TestHandler)]);

        var executor = CreateExecutor(scopeFactory, registry);

        await executor.ExecuteAsync(new TestNotification());

        scopeFactory.Received(2).CreateScope();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenStrategyInvalid()
    {
        SetupDependencies(out _, out _, out var registry, out var scopeFactory);

        registry.GetHandlersForNotification(typeof(TestNotification)).Returns([typeof(TestHandler)]);

        var executor = CreateExecutor(scopeFactory, registry);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            executor.ExecuteAsync(
                new TestNotification(),
                (NotificationExecutionStrategy)999));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseRetryPolicy()
    {
        SetupDependencies(out var scope, out var provider, out var registry, out var scopeFactory);
        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);

        var handler = new TestHandler();
        provider.GetService(typeof(TestHandler)).Returns(handler);
        registry.GetHandlersForNotification(typeof(TestNotification)).Returns([typeof(TestHandler)]);

        var retry = Substitute.For<INotificationRetryPolicy>();
        _ = retry.ExecuteAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var action = ci.Arg<Func<CancellationToken, Task>>();
                return new ValueTask(action(CancellationToken.None));
            });

        var executor = CreateExecutor(scopeFactory, registry, retry);

        await executor.ExecuteAsync(new TestNotification());

        await retry.Received(1)
            .ExecuteAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>());
    }

    private static NotificationExecutor CreateExecutor(
        IServiceScopeFactory scopeFactory, INotificationHandlerRegistry registry, INotificationRetryPolicy? retryPolicy = null)
    {
        retryPolicy ??= new NoRetryNotificationPolicy();

        return new NotificationExecutor(scopeFactory, registry, retryPolicy);
    }

    private static void SetupDependencies(
        out IServiceScope scope,
        out IServiceProvider provider,
        out INotificationHandlerRegistry registry,
        out IServiceScopeFactory scopeFactory)
    {
        scopeFactory = Substitute.For<IServiceScopeFactory>();
        scope = Substitute.For<IServiceScope>();
        provider = Substitute.For<IServiceProvider>();
        registry = Substitute.For<INotificationHandlerRegistry>();
    }
}
