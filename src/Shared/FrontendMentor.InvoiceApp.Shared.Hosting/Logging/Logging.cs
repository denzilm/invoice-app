using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace FrontendMentor.InvoiceApp.Shared.Hosting.Logging;

public static class Logging
{
    public static void Setup(IHostEnvironment environment, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(configuration);

        if (environment.IsDevelopment())
        {
            Log.Logger = CreateCommonLoggerConfiguration(configuration)
                .MinimumLevel.Verbose()
                .WriteTo.Console(
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3} {MemoryUsage} - {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code)
                .CreateLogger();
        }
        else
        {
            Log.Logger = CreateCommonLoggerConfiguration(configuration)
                .MinimumLevel.Information()
                .CreateLogger();
        }
    }

    private static LoggerConfiguration CreateCommonLoggerConfiguration(IConfiguration configuration)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override(nameof(Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware), LogEventLevel.Fatal) // Avoid logging exceptions handled by the Global Exception Handler
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithMemoryUsage()
            .WriteTo.Seq(serverUrl: configuration.GetConnectionString("Seq")!);
    }
}
