using System.Reflection;
using FrontendMentor.InvoiceApp.Messaging.Abstractions;

namespace FrontendMentor.InvoiceApp.Messaging;

public sealed class MessageRegistry : IMessageRegistry
{
    private readonly HashSet<MessageDescriptor> _registeredTypes = [];

    public MessageRegistry(params IEnumerable<Assembly> assemblies)
    {
        var messageTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(t => !t.IsAbstract && typeof(IMessage).IsAssignableFrom(t));

        foreach (var messageType in messageTypes)
        {
            if (Activator.CreateInstance(messageType) is IMessage message)
                _registeredTypes.Add(new MessageDescriptor(messageType, message.Name, message.Version));
        }
    }

    public IReadOnlyList<MessageDescriptor> GetRegisteredTypes() => _registeredTypes.ToList();

    public Type Resolve(string name, int version)
    {
        var descriptor = _registeredTypes.FirstOrDefault(t => t.Name == name && t.Version == version);
        if (descriptor is null)
            throw new InvalidOperationException($"Message type with name '{name}' and '{version}' is not registered");

        return descriptor.MessageType;
    }
}
