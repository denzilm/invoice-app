using Amazon.CDK;
using FrontendMentor.InvoiceApp.Infrastructure;
using FrontendMentor.InvoiceApp.Infrastructure.Global;

var app = new App();

var env = System.Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "dev";
var config = new AppConfig(env);

_ = new GlobalStack(app, "invoice-app-global", config, new StackProps
{
    StackName = "invoice-app-global",
    Env = new Amazon.CDK.Environment
    {
        Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
        Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") ?? "ap-southeast-2"
    }
});

app.Synth();
