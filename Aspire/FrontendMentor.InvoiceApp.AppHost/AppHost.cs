using static FrontendMentor.InvoiceApp.AspireUtilities.AspireConstants;

var builder = DistributedApplication.CreateBuilder(args);

var sqlAdminPassword = builder.AddParameter("SqlAdminPassword", secret: true);
var sqlServer = builder.AddSqlServer("sql", sqlAdminPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("sql_data");

var authDb = sqlServer.AddDatabase(Databases.AuthDb);

var redisPassword = builder.AddParameter("RedisPassword", secret: true);
var cache = builder.AddRedis(Caches.AppCache, password: redisPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("redis_data")
    .WithRedisInsight()
    .WithPersistence(interval: TimeSpan.FromMinutes(5), keysChangedThreshold: 100);

var identityMigrator = builder.AddProject<Projects.IdentityMigrator>(Migrators.IdentityMigrator)
    .WithReference(authDb)
    .WaitFor(authDb)
    .WithParentRelationship(sqlServer)
    .ExcludeFromManifest();

builder.AddProject<Projects.IdentityApi>(Apis.IdentityApi)
    .WithReference(authDb)
    .WithReference(identityMigrator)
    .WithReference(cache)
    .WaitFor(identityMigrator)
    .WaitFor(cache);

builder.Build().Run();
