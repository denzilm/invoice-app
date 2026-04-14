using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;

public sealed record DeadLetterTestMessage : IMessage
{
    public string Name => nameof(DeadLetterTestMessage);
    public int Version => 1;
    public string Content { get; set; } = "";
}
