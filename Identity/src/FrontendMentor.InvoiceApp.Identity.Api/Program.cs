using System.Text.Json;
using System.Text.Json.Serialization;
using FrontendMentor.InvoiceApp.Identity.Infrastructure;
using FrontendMentor.InvoiceApp.Identity.Infrastructure.IdentityPersistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using static FrontendMentor.InvoiceApp.AspireUtilities.AspireConstants;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.AddServiceDefaults();

var dbSecretJson = Environment.GetEnvironmentVariable("DB_SECRET");
if (dbSecretJson is not null)
{
    var secret = JsonSerializer.Deserialize<DbSecret>(dbSecretJson)!;
    configuration[$"ConnectionStrings:{Databases.AuthDb}"] =
        $"Server={secret.Host},{secret.Port}; Database={Databases.AuthDb};" +
        $"User Id={secret.Username};Password={secret.Password};TrustServerCertificate=true";
}

builder.AddSqlServerDbContext<AuthDbContext>(Databases.AuthDb);
services.AddProblemDetails();
services.AddIdentityServices();
var dataProtection = services
    .AddDataProtection()
    .SetApplicationName(Apis.IdentityApi);

if (builder.Environment.IsProduction())
{
    dataProtection
        .PersistKeysToAWSSystemsManager($"{Apis.IdentityApi}/DataProtection/Keys");
}
else
{
    builder.AddRedisClient(Caches.AppCache);
    services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
    {
        return new ConfigureOptions<KeyManagementOptions>(options =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            var database = multiplexer.GetDatabase();
            options.XmlRepository = new RedisXmlRepository(() => database, $"{Apis.IdentityApi}-DataProtection-Keys");
        });
    });
}

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/", (AuthDbContext context) => $"We have currently '{context.Users.ToList().Count}' users");

app.MapHealthChecks("/healthz");

app.Services.GetRequiredService<AuthDbContext>().Database.Migrate();

app.Run();

internal sealed record DbSecret
{
    [JsonPropertyName("host")]
    public required string Host { get; init; }
    [JsonPropertyName("port")]
    public required int Port { get; init; }
    [JsonPropertyName("username")]
    public required string Username { get; init; }
    [JsonPropertyName("password")]
    public required string Password { get; init; }
}
