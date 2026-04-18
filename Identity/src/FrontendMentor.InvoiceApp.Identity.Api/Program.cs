using FrontendMentor.InvoiceApp.Identity.Infrastructure;
using FrontendMentor.InvoiceApp.Identity.Infrastructure.IdentityPersistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using static FrontendMentor.InvoiceApp.AspireUtilities.AspireConstants;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<AuthDbContext>(Databases.AuthDb);
builder.AddRedisClient(Caches.AppCache);
services.AddProblemDetails();
services.AddIdentityServices();
services.AddDataProtection().SetApplicationName(Apis.IdentityApi);
services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
{
    return new ConfigureOptions<KeyManagementOptions>(options =>
    {
        var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
        var database = multiplexer.GetDatabase();
        options.XmlRepository = new RedisXmlRepository(() => database, $"{Apis.IdentityApi}-DataProtection-Keys");
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/", () => "Hello World!");

app.Run();
