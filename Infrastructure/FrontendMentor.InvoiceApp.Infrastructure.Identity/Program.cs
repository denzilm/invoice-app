using Amazon.CDK;
using FrontendMentor.InvoiceApp.Infrastructure;
using FrontendMentor.InvoiceApp.Infrastructure.Identity;
using Environment = Amazon.CDK.Environment;

var app = new App();

var env = System.Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "development";
var config = new AppConfig(env)
{
    ImageTag = System.Environment.GetEnvironmentVariable("IMAGE_TAG") ?? "latest",
    MigrationTag = System.Environment.GetEnvironmentVariable("MIGRATION_TAG") ?? "latest",
    GitCommit = System.Environment.GetEnvironmentVariable("GIT_COMMIT") ?? "unknown",
    IsInitialDeploy = System.Environment.GetEnvironmentVariable("INITIAL_DEPLOY") == "true"
};

var stack = new IdentityStack(app, $"invoice-app-identity-{env}", config, new StackProps
{
    StackName = $"invoice-app-identity-{env}",
    Env = new Environment
    {
        Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
        Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") ?? "ap-southeast-2"
    }
});

Tags.Of(stack).Add("env", env);

app.Synth();
