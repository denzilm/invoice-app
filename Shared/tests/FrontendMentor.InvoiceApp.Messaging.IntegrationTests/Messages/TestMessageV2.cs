using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.IntegrationTests.Messages;

public sealed record TestMessageV2 : IMessage
{
    public string Name => nameof(TestMessageV2);
    public int Version => 2;
    public string Content { get; set; } = "";
    public string Extra { get; set; } = "";
}
