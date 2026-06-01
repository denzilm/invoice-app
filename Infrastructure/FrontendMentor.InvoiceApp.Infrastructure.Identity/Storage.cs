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
    public Storage(Construct scope, string id, AppConfig config, IVpc vpc, Parameters parameters)
        : base(scope, id)
    {
        var databaseSecurityGroup = new SecurityGroup(this, $"identity-database-security-group-{config.Environment}", new SecurityGroupProps
        {
            Vpc = vpc,
            SecurityGroupName = $"identity-database-security-group-{config.Environment}",
            Description = "Security group for the identity database",
            AllowAllOutbound = true
        });

        var migrationSecurityGroup = new SecurityGroup(
            this, $"identity-database-migration-security-group-{config.Environment}", new SecurityGroupProps
            {
                Vpc = vpc,
                SecurityGroupName = $"identity-database-migration-security-group-{config.Environment}",
                Description = "Migration task security group for the identity database",
                AllowAllOutbound = true
            });

        databaseSecurityGroup.AddIngressRule(migrationSecurityGroup, Port.Tcp(1433), "Allow database access from the migration task security group");

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

        DatabaseSecret = databaseInstance.Secret;
        DatabaseSecurityGroup = databaseSecurityGroup;

        _ = new StringParameter(this, "invoice-app-identity-database-migration-param", new StringParameterProps
        {
            ParameterName = $"{parameters.Network}/migration-sg-id",
            StringValue = migrationSecurityGroup.SecurityGroupId,
            Description = "Migration task security group Id for the identity database"
        });
    }

    public ISecret? DatabaseSecret { get; }
    public ISecurityGroup DatabaseSecurityGroup { get; }
}
