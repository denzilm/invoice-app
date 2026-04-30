namespace FrontendMentor.InvoiceApp.Infrastructure;

public sealed record AppConfig(string Environment, string ImageTag = "", string MigrationTag = "", string GitCommit = "unknown")
{
    public static string AppName => "invoice-app";
}
