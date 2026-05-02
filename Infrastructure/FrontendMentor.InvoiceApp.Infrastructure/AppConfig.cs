namespace FrontendMentor.InvoiceApp.Infrastructure;

public sealed record AppConfig(string Environment)
{
    public static string AppName => "invoice-app";

    public string ImageTag { get; init; } = "";
    public string MigrationTag { get; init; } = "";
    public string GitCommit { get; init; } = "unknown";
    public bool IsInitialDeploy { get; init; }
}
