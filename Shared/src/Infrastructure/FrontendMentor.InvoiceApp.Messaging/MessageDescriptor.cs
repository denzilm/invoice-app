namespace FrontendMentor.InvoiceApp.Messaging;

public sealed record MessageDescriptor(Type MessageType, string Name, int Version);
