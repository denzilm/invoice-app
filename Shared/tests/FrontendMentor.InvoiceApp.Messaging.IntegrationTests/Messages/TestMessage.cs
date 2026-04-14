using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;

public sealed record TestMessage : IMessage
{
    public string Name => nameof(TestMessage);
    public int Version => 1;

    public required string Content { get; init; }
}
