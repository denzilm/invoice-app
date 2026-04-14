using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.Tests.Messages;

public sealed record TestMessageV1 : IMessage
{
    public string Name => nameof(TestMessageV1);
    public int Version => 1;
}
