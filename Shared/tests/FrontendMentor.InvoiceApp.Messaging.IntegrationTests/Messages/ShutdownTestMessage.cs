using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;

public sealed class ShutdownTestMessage : IMessage
{
    public string Name => "ShutdownTestMessage";
    public int Version => 1;
}
