using Amazon.CDK;
using FrontendMentor.InvoiceApp.Infrastructure;
using FrontendMentor.InvoiceApp.Infrastructure.Core;
using Environment = Amazon.CDK.Environment;

var app = new App();

var env = app.Node.TryGetContext("env").ToString() ?? "dev";
var config = new AppConfig(env);

var stack = new InvoiceAppStack(app, $"invoice-app-{env}", config, new StackProps
{
    StackName = $"invoice-app-{env}",
    Env = new Environment
    {
        Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
        Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") ?? "ap-southeast-2"
    }
});

Tags.Of(stack).Add("env", env);

app.Synth();
