using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging.Tests.Messages;

public abstract record AbstractMessage : IMessage
{
    public abstract string Name { get; }
    public abstract int Version { get; }
}
