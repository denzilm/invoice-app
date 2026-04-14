using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;

public sealed record RetryMessage : IMessage
{
    public string Name => nameof(RetryMessage);
    public int Version => 1;
}
