namespace FrontendMentor.InvoiceApp.Messaging.Abstractions;

public interface IMessageHandler : IAsyncDisposable
{
    Task StartAsync(MessageDescriptor descriptor, CancellationToken cancellationToken = default);
}
