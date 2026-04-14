using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;

public sealed record ConcurrentTestMessage : IMessage
{
    public string Name => nameof(ConcurrentTestMessage);
    public int Version => 1;
    public int Index { get; set; }
}
