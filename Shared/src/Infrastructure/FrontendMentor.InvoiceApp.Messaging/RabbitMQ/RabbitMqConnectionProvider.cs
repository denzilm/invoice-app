using RabbitMQ.Client;

namespace FrontendMentor.InvoiceApp.Messaging.RabbitMQ;

public sealed class RabbitMqConnectionProvider : IAsyncDisposable
{
    private readonly Lazy<Task<IConnection>> _connection;

    public RabbitMqConnectionProvider(string connectionString)
    {
        _connection = new Lazy<Task<IConnection>>(() => CreateConnectionAsync(connectionString));
    }

    public async Task<IConnection> GetConnectionAsync() => await _connection.Value;

    private static async Task<IConnection> CreateConnectionAsync(string connectionString)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        return await factory.CreateConnectionAsync();
    }

    public async ValueTask DisposeAsync() => (await _connection.Value).Dispose();
}
