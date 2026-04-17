using FrontendMentor.InvoiceApp.Identity.Infrastructure.IdentityPersistence;
using FrontendMentor.InvoiceApp.Identity.MigrationService;
using static FrontendMentor.InvoiceApp.AspireUtilities.AspireConstants;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

builder.AddServiceDefaults();

services.AddHostedService<MigrationHostedService>();
services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(MigrationHostedService.ActivitySourceName));

builder.AddSqlServerDbContext<AuthDbContext>(Databases.AuthDb);

var host = builder.Build();
host.Run();
