using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;

public sealed record MultiHandlerTestMessage : IMessage
{
    public string Name => nameof(MultiHandlerTestMessage);
    public int Version => 1;
}
