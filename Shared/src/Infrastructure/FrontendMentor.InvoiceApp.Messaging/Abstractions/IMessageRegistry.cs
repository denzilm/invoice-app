namespace FrontendMentor.InvoiceApp.Messaging.Abstractions;

public interface IMessageRegistry
{
    IReadOnlyList<MessageDescriptor> GetRegisteredTypes();
    Type Resolve(string name, int version);
}
