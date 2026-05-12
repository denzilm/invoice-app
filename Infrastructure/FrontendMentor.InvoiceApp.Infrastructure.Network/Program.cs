using Amazon.CDK;
using FrontendMentor.InvoiceApp.Infrastructure;
using FrontendMentor.InvoiceApp.Infrastructure.Core;
using Environment = Amazon.CDK.Environment;

var app = new App();

var env = System.Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "dev";
var config = new AppConfig(env);

var stack = new NetworkStack(app, $"invoice-app-network-{env}", config, new StackProps
{
    StackName = $"invoice-app-network-{env}",
    Env = new Environment
    {
        Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
        Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") ?? "ap-southeast-2"
    }
});

Tags.Of(stack).Add("env", env);

app.Synth();
