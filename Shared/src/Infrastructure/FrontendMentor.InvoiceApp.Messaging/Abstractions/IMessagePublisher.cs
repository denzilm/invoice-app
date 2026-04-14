namespace FrontendMentor.InvoiceApp.Messaging.Abstractions;

public interface IMessagePublisher : IAsyncDisposable
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : IMessage;
}
