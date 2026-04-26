using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace FrontendMentor.InvoiceApp.Infrastructure;

public sealed class SharedResources : Construct
{
    public SharedResources(Construct scope, string id, AppConfig config)
        : base(scope, id)
    {
        var parameters = new Parameters(config);
        ApplicationVpc = Vpc.FromLookup(this, $"invoice-app-vpc-{config.Environment}", new VpcLookupOptions
        {
            VpcId = StringParameter.ValueFromLookup(this, parameters.VpcId),
        });

        Cluster = Amazon.CDK.AWS.ECS.Cluster.FromClusterAttributes(
            this, $"invoice-app-cluster-{config.Environment}", new ClusterAttributes
            {
                Vpc = ApplicationVpc,
                ClusterName = $"invoice-app-cluster-{config.Environment}",
                ClusterArn = StringParameter.ValueForStringParameter(this, parameters.ClusterArn)
            });

        IdentityRepository = Repository.FromRepositoryName(
            this,
            "invoice-app-identity-api",
            StringParameter.ValueForStringParameter(this, parameters.EcrIdentityRepositoryName)
        );

        ExecutionRole = Role.FromRoleArn(
            this, $"invoice-app-execution-role-{config.Environment}", StringParameter.ValueForStringParameter(this, parameters.ExecutionRoleArn));
    }

    public IVpc ApplicationVpc { get; }
    public IRepository IdentityRepository { get; }
    public ICluster Cluster { get; }
    public IRole ExecutionRole { get; }
}
