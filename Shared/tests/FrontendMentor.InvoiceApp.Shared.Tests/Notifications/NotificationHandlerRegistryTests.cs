using FrontendMentor.InvoiceApp.Shared.Notifications;
using FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestHandlers;
using FrontendMentor.InvoiceApp.Shared.Tests.Notifications.TestNotifications;

namespace FrontendMentor.InvoiceApp.Shared.Tests.Notifications;

public sealed class NotificationHandlerRegistryTests
{
    [Fact]
    public void GetHandlersForNotification_ShouldThrow_WhenTypeIsNotNotification()
    {
        var registry = new NotificationHandlerRegistry([typeof(string).Assembly]);

        var ex = Assert.Throws<ArgumentException>(() =>
            registry.GetHandlersForNotification(typeof(string)));

        Assert.Contains("does not implement INotification", ex.Message);
    }

    [Fact]
    public void GetHandlersForNotification_ShouldReturnMatchingHandlers()
    {
        var registry = new NotificationHandlerRegistry([typeof(TestHandler).Assembly]);

        var handlers = registry.GetHandlersForNotification(typeof(TestNotification));

        Assert.Contains(typeof(TestHandler), handlers);
    }

    [Fact]
    public void GetHandlersForNotification_ShouldNotReturnHandlersForOtherNotifications()
    {
        var registry = new NotificationHandlerRegistry([typeof(OtherHandler).Assembly]);

        var handlers = registry.GetHandlersForNotification(typeof(TestNotification));

        Assert.DoesNotContain(typeof(OtherHandler), handlers);
    }

    [Fact]
    public void GetHandlersForNotification_ShouldIgnoreAbstractHandlers()
    {
        var registry = new NotificationHandlerRegistry([typeof(AbstractHandler).Assembly]);

        var handlers = registry.GetHandlersForNotification(typeof(TestNotification));

        Assert.DoesNotContain(typeof(AbstractHandler), handlers);
    }

    [Fact]
    public void GetHandlersForNotification_ShouldIgnoreInterfaces()
    {
        var registry = new NotificationHandlerRegistry([typeof(ITestHandler).Assembly]);

        var handlers = registry.GetHandlersForNotification(typeof(TestNotification));

        Assert.DoesNotContain(typeof(ITestHandler), handlers);
    }

    [Fact]
    public void GetHandlersForNotification_ShouldReturnEmpty_WhenNoHandlersExist()
    {
        var registry = new NotificationHandlerRegistry([typeof(string).Assembly]);

        var handlers = registry.GetHandlersForNotification(typeof(TestNotification));

        Assert.Empty(handlers);
    }

    [Fact]
    public void GetHandlersForNotification_ShouldCacheResults()
    {
        var registry = new NotificationHandlerRegistry([typeof(TestHandler).Assembly]);

        var first = registry.GetHandlersForNotification(typeof(TestNotification));
        var second = registry.GetHandlersForNotification(typeof(TestNotification));

        Assert.Same(first, second); // same reference = cached
    }

    [Fact]
    public void GetHandlersForNotification_ShouldReturnAllHandlers()
    {
        var registry = new NotificationHandlerRegistry([typeof(TestHandler).Assembly]);

        var handlers = registry.GetHandlersForNotification(typeof(TestNotification));

        Assert.Contains(typeof(TestHandler), handlers);
        Assert.Contains(typeof(SecondHandler), handlers);
        Assert.Equal(2, handlers.Count);
    }
}
