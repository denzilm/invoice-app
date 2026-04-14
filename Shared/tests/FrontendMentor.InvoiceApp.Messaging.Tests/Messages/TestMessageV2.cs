using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.Tests.Messages;

public sealed record TestMessageV2 : IMessage
{
    public string Name => nameof(TestMessageV2);
    public int Version => 2;
}
