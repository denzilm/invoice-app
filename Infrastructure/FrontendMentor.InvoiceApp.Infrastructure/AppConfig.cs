namespace FrontendMentor.InvoiceApp.Infrastructure;

public sealed record AppConfig(string Environment, string ImageTag = "")
{
    public static string AppName => "invoice-app";
}
