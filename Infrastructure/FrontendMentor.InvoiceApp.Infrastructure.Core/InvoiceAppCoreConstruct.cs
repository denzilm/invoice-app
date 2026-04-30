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
    public ICluster Cluster { get; }

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

        var identityRepository = new Repository(this, "invoice-app-identity-api", new RepositoryProps
        {
            RepositoryName = "invoice-app-identity-api",
            ImageTagMutability = TagMutability.MUTABLE,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        var identityMigrationRepository = new Repository(this, "invoice-app-identity-api-migration", new RepositoryProps
        {
            RepositoryName = "invoice-app-identity-api-migration",
            ImageTagMutability = TagMutability.MUTABLE,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        Cluster = new Cluster(this, "invoice-app-cluster", new ClusterProps
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

        _ = new StringParameter(this, "IdentityApiEcrRepository", new StringParameterProps
        {
            ParameterName = parameters.EcrIdentityRepositoryName,
            StringValue = identityRepository.RepositoryName
        });

        _ = new StringParameter(this, "IdentityApiMigrationEcrRepository", new StringParameterProps
        {
            ParameterName = parameters.EcrIdentityMigrationRepositoryName,
            StringValue = identityMigrationRepository.RepositoryName
        });

        _ = new StringParameter(this, "Cluster", new StringParameterProps
        {
            ParameterName = parameters.ClusterArn,
            StringValue = Cluster.ClusterArn
        });

        _ = new StringParameter(this, "ExecutionRole", new StringParameterProps
        {
            ParameterName = parameters.ExecutionRoleArn,
            StringValue = executionRole.RoleArn
        });

        _ = new StringParameter(this, "invoice-app-private-subnetIds", new StringParameterProps
        {
            ParameterName = $"{parameters.Network}/private-subnet-ids",
            StringValue = string.Join(",",
                vpc.PrivateSubnets.Select(subnet => subnet.SubnetId)),
            Description = "Comma-separated list of private subnet IDs for the application VPC",
        });
    }
}
