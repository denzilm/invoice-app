using FrontendMentor.InvoiceApp.Identity.Infrastructure;
using FrontendMentor.InvoiceApp.Identity.Infrastructure.IdentityPersistence;
using static FrontendMentor.InvoiceApp.AspireUtilities.AspireConstants;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<AuthDbContext>(Databases.AuthDb);
services.AddProblemDetails();
services.AddIdentityServices();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!")
    .AllowAnonymous();

app.Run();
