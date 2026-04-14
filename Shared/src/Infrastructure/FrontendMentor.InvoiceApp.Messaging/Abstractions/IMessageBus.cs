namespace FrontendMentor.InvoiceApp.Messaging.Abstractions;

public interface IMessageBus : IAsyncDisposable
{
    Task<IMessagePublisher> CreatePublisherAsync(CancellationToken cancellationToken = default);
    Task<IMessageHandler> CreateListenerAsync(string consumer, CancellationToken cancellationToken = default);
}
