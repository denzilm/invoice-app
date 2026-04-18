using static FrontendMentor.InvoiceApp.AspireUtilities.AspireConstants;

var builder = DistributedApplication.CreateBuilder(args);

var sqlAdminPassword = builder.AddParameter("SqlAdminPassword", secret: true);
var sqlServer = builder.AddSqlServer("sql", sqlAdminPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("sql_data");

var authDb = sqlServer.AddDatabase(Databases.AuthDb);

builder.AddProject<Projects.IdentityMigrator>(Migrators.IdentityMigrator)
    .WithReference(authDb)
    .WaitFor(authDb)
    .WithParentRelationship(sqlServer)
    .ExcludeFromManifest();

builder.Build().Run();
