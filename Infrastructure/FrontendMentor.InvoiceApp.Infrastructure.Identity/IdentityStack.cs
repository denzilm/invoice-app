using Amazon.CDK;
using Constructs;

namespace FrontendMentor.InvoiceApp.Infrastructure.Identity;

public sealed class IdentityStack : Stack
{
    internal IdentityStack(Construct scope, string id, AppConfig config, IStackProps? props = null)
        : base(scope, id, props)
    {
        var shared = new SharedResources(this, $"invoice-app-shared-resources-{config.Environment}", config);
        var storage = new Storage(this, $"invoice-app-identity-storage-{config.Environment}", config, shared.ApplicationVpc);
        _ = new IdentityApi(this, $"invoice-app-identity-api-{config.Environment}", config, new IdentityApiProps
        {
            Vpc = shared.ApplicationVpc,
            IdentityRepository = shared.IdentityRepository,
            DatabaseSecret = storage.DatabaseSecret,
            DatabaseSecurityGroup = storage.DatabaseSecurityGroup,
            Cluster = shared.Cluster,
            ExecutionRole = shared.ExecutionRole
        });
    }
}
