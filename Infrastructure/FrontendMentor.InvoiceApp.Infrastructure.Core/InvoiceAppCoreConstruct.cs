using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace FrontendMentor.InvoiceApp.Infrastructure.Core;

public sealed class InvoiceAppCoreConstruct : Construct
{
    public InvoiceAppCoreConstruct(Construct scope, string id, AppConfig config)
        : base(scope, id)
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

        var repository = new Repository(this, "invoice-app-identity-api", new RepositoryProps
        {
            RepositoryName = "invoice-app-identity-api",
            ImageTagMutability = TagMutability.MUTABLE,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        var cluster = new Cluster(this, "invoice-app-cluster", new ClusterProps
        {
            Vpc = vpc,
            ClusterName = $"invoice-app-cluster-{config.Environment}"
        });

        var executionRole = new Role(this, "invoice-app-execution-role", new RoleProps
        {
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
        });

        executionRole.AddManagedPolicy(
            ManagedPolicy
                .FromManagedPolicyArn(
                    this, "TaskExecutionPolicy", "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"));

        var parameters = new Parameters(config);
        _ = new StringParameter(this, "VpcIdParameter", new StringParameterProps
        {
            ParameterName = parameters.VpcId,
            StringValue = vpc.VpcId
        });

        _ = new StringParameter(this, "EcrRepository", new StringParameterProps
        {
            ParameterName = parameters.EcrIdentityRepositoryName,
            StringValue = repository.RepositoryName
        });

        _ = new StringParameter(this, "Cluster", new StringParameterProps
        {
            ParameterName = parameters.ClusterArn,
            StringValue = cluster.ClusterArn
        });

        _ = new StringParameter(this, "ExecutionRole", new StringParameterProps
        {
            ParameterName = parameters.ExecutionRoleArn,
            StringValue = executionRole.RoleArn
        });
    }
}
