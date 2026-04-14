using FrontendMentor.InvoiceApp.Shared.Notifications;

namespace FrontendMentor.InvoiceApp.Messaging.Abstractions;

public interface IMessage : INotification
{
    string Name { get; }
    int Version { get; }
}
