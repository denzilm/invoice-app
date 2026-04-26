using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK.AWS.SSM;
using Constructs;
using InstanceType = Amazon.CDK.AWS.EC2.InstanceType;

namespace FrontendMentor.InvoiceApp.Infrastructure.Identity;

public sealed class Storage : Construct
{
    public Storage(Construct scope, string id, AppConfig config, IVpc vpc)
        : base(scope, id)
    {
        var databaseSecurityGroup = new SecurityGroup(this, $"identity-database-security-group-{config.Environment}", new SecurityGroupProps
        {
            Vpc = vpc,
            SecurityGroupName = $"identity-database-security-group-{config.Environment}",
            Description = "Security group for the identity database",
            AllowAllOutbound = true
        });

        var databaseInstance = new DatabaseInstance(this, $"identity-database-instance-{config.Environment}", new DatabaseInstanceProps
        {
            Engine = DatabaseInstanceEngine.SqlServerEx(new SqlServerExInstanceEngineProps
            {
                Version = SqlServerEngineVersion.VER_16
            }),
            InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MICRO),
            Vpc = vpc,
            MultiAz = false,
            AllocatedStorage = 20,
            StorageType = StorageType.GP3,
            Credentials = Credentials.FromGeneratedSecret("invoice_app_user"),
            RemovalPolicy = RemovalPolicy.DESTROY,
            DeleteAutomatedBackups = true,
            SecurityGroups = [databaseSecurityGroup],
            SubnetGroup = new SubnetGroup(this, $"invoice-app-database-subnet-group-{config.Environment}", new SubnetGroupProps
            {
                Vpc = vpc,
                SubnetGroupName = $"identity-database-subnet-group-{config.Environment}",
                Description = "Subnet group for the identity database",
                VpcSubnets = new SubnetSelection
                {
                    Subnets = vpc.IsolatedSubnets
                }
            })
        });

        var databaseEndPointParameter = new StringParameter(
            this,
            $"invoice-app-identity-database-endpoint-parameter-{config.Environment}",
            new StringParameterProps
            {
                ParameterName = $"/{AppConfig.AppName}/{config.Environment}/identity-database-endpoint",
                StringValue = databaseInstance.DbInstanceEndpointAddress,
                Description = "Database endpoint for the identity database"
            });

        DatabaseSecret = databaseInstance.Secret;
        DatabaseEndPoint = databaseEndPointParameter;
        DatabaseSecurityGroup = databaseSecurityGroup;
    }

    public ISecret? DatabaseSecret { get; }
    public IStringParameter DatabaseEndPoint { get; }
    public ISecurityGroup DatabaseSecurityGroup { get; }
}
