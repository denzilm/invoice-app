using Amazon.CDK;
using Constructs;

namespace FrontendMentor.InvoiceApp.Infrastructure.Core;

public sealed class InvoiceAppStack : Stack
{
    internal InvoiceAppStack(Construct scope, string id, AppConfig config, IStackProps? props = null)
        : base(scope, id, props)
    {
        var coreConstruct = new InvoiceAppCoreConstruct(this, $"networking-{config.Environment}", config);

        _ = new CfnOutput(this, "ClusterName", new CfnOutputProps
        {
            Value = coreConstruct.Cluster.ClusterName
        });
    }
}
