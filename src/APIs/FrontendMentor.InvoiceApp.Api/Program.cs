using FrontendMentor.InvoiceApp.Shared.Hosting.Extensions;
using FrontendMentor.InvoiceApp.Shared.Hosting.Logging;
using Serilog;

using static FrontendMentor.InvoiceApp.Api.Utils.Constants;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();
    var seqServerUrl = builder.Configuration.GetRequiredValue(ConfigurationKeys.SeqServerUrl);
    Logging.Setup(builder.Environment, seqServerUrl);

    Log.Information("Starting Invoice App Service");

    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
