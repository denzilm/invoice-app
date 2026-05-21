using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace FrontendMentor.InvoiceApp.Infrastructure.Core;

public sealed class NetworkStack : Stack
{
    internal NetworkStack(Construct scope, string id, AppConfig config, IStackProps? props = null)
        : base(scope, id, props)
    {
        var vpc = new Vpc(this, $"vpc-{config.Environment}", new VpcProps
        {
            MaxAzs = 3,
            NatGateways = 1,
            SubnetConfiguration =
            [
                new SubnetConfiguration
                {
                    Name = "public",
                    SubnetType = SubnetType.PUBLIC
                },
                new SubnetConfiguration
                {
                    Name = "private",
                    SubnetType = SubnetType.PRIVATE_WITH_EGRESS
                },
                new SubnetConfiguration()
                {
                    Name = "isolated",
                    SubnetType = SubnetType.PRIVATE_ISOLATED
                }
            ]
        });

        var cluster = new Cluster(this, "invoice-app-cluster", new ClusterProps
        {
            Vpc = vpc,
            ClusterName = $"invoice-app-cluster-{config.Environment}"
        });

        var parameters = new Parameters(config);
        _ = new StringParameter(this, "VpcIdParameter", new StringParameterProps
        {
            ParameterName = parameters.VpcId,
            StringValue = vpc.VpcId
        });

        _ = new StringParameter(this, "Cluster", new StringParameterProps
        {
            ParameterName = parameters.ClusterArn,
            StringValue = cluster.ClusterArn
        });

        _ = new StringParameter(this, "invoice-app-private-subnetIds", new StringParameterProps
        {
            ParameterName = $"{parameters.Network}/private-subnet-ids",
            StringValue = string.Join(",",
                vpc.PrivateSubnets.Select(subnet => subnet.SubnetId)),
            Description = "Comma-separated list of private subnet IDs for the application VPC",
        });

        _ = new CfnOutput(this, "ClusterName", new CfnOutputProps
        {
            Value = cluster.ClusterName
        });
    }
}
