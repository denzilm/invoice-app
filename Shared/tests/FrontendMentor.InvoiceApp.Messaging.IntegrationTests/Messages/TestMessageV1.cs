using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;

public sealed record TestMessageV1 : IMessage
{
    public string Name => nameof(TestMessageV1);
    public int Version => 1;

    public required string Content { get; init; }
}
