using System.Text.Json;
using System.Text.Json.Serialization;
using FrontendMentor.InvoiceApp.Identity.Infrastructure;
using FrontendMentor.InvoiceApp.Identity.Infrastructure.AppPersistence;
using FrontendMentor.InvoiceApp.Identity.Infrastructure.IdentityPersistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using static FrontendMentor.InvoiceApp.AspireUtilities.AspireConstants;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.AddServiceDefaults();

var dbAuthSecretJson = Environment.GetEnvironmentVariable("DB_AUTH_SECRET");
var dbAppSecretJson = Environment.GetEnvironmentVariable("DB_APP_SECRET");
var dbSecretJson = Environment.GetEnvironmentVariable("DB_SECRET");
var authDbName = Environment.GetEnvironmentVariable("AUTH_DB_NAME") ?? "IdentityAuth";
var identityDbName = Environment.GetEnvironmentVariable("IDENTITY_DB_NAME") ?? "IdentityApp";
if (dbSecretJson is not null)
{
    var dbSecret = JsonSerializer.Deserialize<DbSecret>(dbSecretJson)!;

    configuration[$"ConnectionStrings:{Databases.AuthDb}"] =
        BuildConnectionString(dbSecret, authDbName);

    configuration[$"ConnectionStrings:{Databases.IdentityAppDb}"] =
        BuildConnectionString(dbSecret, identityDbName);
}
else if (TryGetDatabaseSettings(out var dbHost, out var dbPort, out var dbUsername, out var dbPassword))
{
    configuration[$"ConnectionStrings:{Databases.AuthDb}"] =
        BuildConnectionStringFromParts(dbHost, dbPort, authDbName, dbUsername, dbPassword);

    configuration[$"ConnectionStrings:{Databases.IdentityAppDb}"] =
        BuildConnectionStringFromParts(dbHost, dbPort, identityDbName, dbUsername, dbPassword);
}
else if (dbAuthSecretJson is not null && dbAppSecretJson is not null)
{
    var authSecret = JsonSerializer.Deserialize<DbSecret>(dbAuthSecretJson)!;
    var appSecret = JsonSerializer.Deserialize<DbSecret>(dbAppSecretJson)!;

    configuration[$"ConnectionStrings:{Databases.AuthDb}"] =
        BuildConnectionString(authSecret, authSecret.Database ?? authDbName);

    configuration[$"ConnectionStrings:{Databases.IdentityAppDb}"] =
        BuildConnectionString(appSecret, appSecret.Database ?? identityDbName);
}

builder.AddSqlServerDbContext<AuthDbContext>(Databases.AuthDb);
builder.AddSqlServerDbContext<IdentityAppDbContext>(Databases.IdentityAppDb);
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

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("live")
});

app.Run();

static string BuildConnectionString(DbSecret secret, string databaseName) =>
    BuildConnectionStringFromParts(secret.Host, secret.Port.ToString(), databaseName, secret.Username, secret.Password);

static string BuildConnectionStringFromParts(string host, string port, string databaseName, string username, string password) =>
    $"Server={host},{port}; Database={databaseName};" +
    $"User Id={username};Password={password};TrustServerCertificate=true";

static bool TryGetDatabaseSettings(
    out string host,
    out string port,
    out string username,
    out string password)
{
    host = Environment.GetEnvironmentVariable("DB_HOST") ?? string.Empty;
    port = Environment.GetEnvironmentVariable("DB_PORT") ?? string.Empty;
    username = Environment.GetEnvironmentVariable("DB_USER") ?? string.Empty;
    password = Environment.GetEnvironmentVariable("DB_PASS") ?? string.Empty;

    return host.Length > 0 && port.Length > 0 && username.Length > 0 && password.Length > 0;
}

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
    [JsonPropertyName("dbInstanceIdentifier")]
    public string? Database { get; init; }
}
