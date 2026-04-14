using FrontendMentor.InvoiceApp.Messaging.Tests.Messages;

namespace FrontendMentor.InvoiceApp.Messaging.Tests;

public sealed class MessageRegistryTests
{
    [Fact]
    public void Registers_Concrete_IMessage_Types()
    {
        var registry = new MessageRegistry(typeof(TestMessageV1).Assembly);

        var types = registry.GetRegisteredTypes();

        Assert.Contains(types, x => x.Name == nameof(TestMessageV1) && x.Version == 1);
    }

    [Fact]
    public void DoesNotRegister_Abstract_IMessage_Types()
    {
        var registry = new MessageRegistry(typeof(AbstractMessage).Assembly);

        var types = registry.GetRegisteredTypes();

        Assert.DoesNotContain(types, x => x.Name == nameof(AbstractMessage) && x.Version == 1);
    }

    [Fact]
    public void DoesNotRegister_NonMessage_Types()
    {
        var registry = new MessageRegistry(typeof(NotAMessage).Assembly);

        var types = registry.GetRegisteredTypes();

        Assert.DoesNotContain(types, x => x.Name == nameof(NotAMessage) && x.Version == 1);
    }

    [Fact]
    public void Resolve_Returns_Correct_Type()
    {
        var registry = new MessageRegistry(typeof(TestMessageV1).Assembly);

        var type = registry.Resolve(nameof(TestMessageV1), 1);

        Assert.Equal(typeof(TestMessageV1), type);
    }

    [Fact]
    public void Resolve_Throws_When_Not_Found()
    {
        var registry = new MessageRegistry(typeof(TestMessageV1).Assembly);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            registry.Resolve("Unknown", 99));

        Assert.Contains("Unknown", ex.Message);
    }

    [Fact]
    public void Resolve_Is_Case_Sensitive()
    {
        var registry = new MessageRegistry(typeof(TestMessageV1).Assembly);

        Assert.Throws<InvalidOperationException>(() =>
            registry.Resolve(nameof(TestMessageV1).ToLower(), 1)); // lower case
    }
}
