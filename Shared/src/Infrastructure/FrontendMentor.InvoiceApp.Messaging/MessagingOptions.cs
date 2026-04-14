using System.Reflection;

namespace FrontendMentor.InvoiceApp.Messaging;

public sealed record MessagingOptions
{
    public MessagingProvider Provider { get; init; } = MessagingProvider.RabbitMq;
    public string ConnectionString { get; init; } = string.Empty;
    public int MaxConcurrentCalls { get; init; } = 10;
    public IEnumerable<Assembly> Assemblies { get; init; } = [];
}

public sealed record OutBoxOptions
{
    public bool Enabled { get; init; } = true;
    public int BatchSize { get; init; } = 100;
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(5);
    public TimeSpan RetentionPeriod { get; init; } = TimeSpan.FromDays(7);
    public int MaxRetryCount { get; init; } = 5;
}
