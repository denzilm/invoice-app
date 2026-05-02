using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK.AWS.SSM;
using Constructs;
using HealthCheck = Amazon.CDK.AWS.ECS.HealthCheck;
using Protocol = Amazon.CDK.AWS.ECS.Protocol;
using Secret = Amazon.CDK.AWS.ECS.Secret;

namespace FrontendMentor.InvoiceApp.Infrastructure.Identity;

public sealed class IdentityApi : Construct
{
    public IdentityApi(Construct scope, string id, AppConfig config, IdentityApiProps props) :
        base(scope, id)
    {
        var loadBalancerSecurityGroup = new SecurityGroup(this, $"load-balancer-security-group-{config.Environment}",
            new SecurityGroupProps
            {
                Vpc = props.Vpc,
                SecurityGroupName = $"load-balancer-security-group-{config.Environment}",
                Description = "Security group for the application load balancer",
                AllowAllOutbound = true
            });
        loadBalancerSecurityGroup.AddIngressRule(Peer.AnyIpv4(), Port.HTTP, "Allow HTTP traffic from anywhere");

        var loadBalancer = new ApplicationLoadBalancer(this, $"load-balancer-{config.Environment}", new ApplicationLoadBalancerProps
        {
            Vpc = props.Vpc,
            LoadBalancerName = $"load-balancer-{config.Environment}",
            InternetFacing = true,
            SecurityGroup = loadBalancerSecurityGroup
        });

        var apiSecurityGroup = new SecurityGroup(this, "invoice-app-identity-api-security-group", new SecurityGroupProps
        {
            Vpc = props.Vpc,
            SecurityGroupName = $"invoice-app-identity-api-security-group-{config.Environment}",
            Description = "Security group for invoice app identity api",
            AllowAllOutbound = true
        });
        apiSecurityGroup
            .AddIngressRule(
                Peer.SecurityGroupId(loadBalancerSecurityGroup.SecurityGroupId), Port.HTTP, "Allows HTTP traffic from the load balancer");

        props.DatabaseSecurityGroup
            .AddIngressRule(
                Peer.SecurityGroupId(apiSecurityGroup.SecurityGroupId), Port.Tcp(1433), "Allows Database access from the api security group");

        var taskRole = new Role(this, "invoice-app-identity-api-task-role", new RoleProps
        {
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
        });
        props.DatabaseSecret!.GrantRead(taskRole);

        taskRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions =
            [
                "ssm:GetParametersByPath",
                "ssm:GetParameters",
                "ssm:GetParameter",
                "ssm:PutParameter"
            ],
            Resources =
            [
                Arn.Format(new ArnComponents
                {
                    Service = "ssm",
                    Resource = "parameter",
                    ResourceName = "IdentityApi/DataProtection/*"
                }, Stack.Of(this))
            ]
        }));

        var parameters = new Parameters(config);
        var serviceRepo = Repository.FromRepositoryName(
            this,
            "invoice-app-identity-api",
            StringParameter.ValueForStringParameter(this, parameters.EcrIdentityRepositoryName)
        );

        var migrationsRepo = Repository.FromRepositoryName(
            this,
            "invoice-app-identity-api-migrations",
            StringParameter.ValueForStringParameter(this, parameters.EcrIdentityMigrationRepositoryName)
        );

        var taskDefinition = new FargateTaskDefinition(this, "invoice-app-identity-api-task-definition",
            new FargateTaskDefinitionProps
            {
                ExecutionRole = props.ExecutionRole,
                TaskRole = taskRole,
            });
        taskDefinition.AddContainer($"invoice-app-identity-api-{config.Environment}", new ContainerDefinitionOptions
        {
            Image = ContainerImage.FromEcrRepository(serviceRepo, config.ImageTag),
            PortMappings =
            [
                new PortMapping
                {
                    Name = "invoice-app-identity-api",
                    ContainerPort = 8080,
                    Protocol = Protocol.TCP
                }
            ],
            ContainerName = $"invoice-app-identity-api-{config.Environment}",
            Secrets = new Dictionary<string, Secret>
            {
                ["DB_SECRET"] = Secret.FromSecretsManager(props.DatabaseSecret!)
            },
            Logging = LogDriver.AwsLogs(new AwsLogDriverProps
            {
                StreamPrefix = $"invoice-app-identity-api-{config.Environment}",
                Mode = AwsLogDriverMode.NON_BLOCKING,
                MaxBufferSize = Size.Mebibytes(25)
            }),
            HealthCheck = new HealthCheck
            {
                Command = ["CMD-SHELL", "wget --no-verbose --tries=1 --spider http://localhost:8080/healthz || exit 1"],
                Interval = Duration.Seconds(30),
                Retries = 10,
                StartPeriod = Duration.Minutes(1),
                Timeout = Duration.Seconds(30),
            }
        });

        var service = new FargateService(this, "invoice-app-identity-api-service", new FargateServiceProps
        {
            Cluster = props.Cluster,
            TaskDefinition = taskDefinition,
            DesiredCount = config.IsInitialDeploy ? 0 : config.Environment == "Production" ? 2 : 1,
            AssignPublicIp = false,
            VpcSubnets = new SubnetSelection
            {
                Subnets = props.Vpc.PrivateSubnets
            },
            MinHealthyPercent = 50,
            SecurityGroups = [apiSecurityGroup],
            EnableExecuteCommand = true
        });

        var migrationTaskDef = new FargateTaskDefinition(this, "invoice-app-identity-migration-task-definition",
            new FargateTaskDefinitionProps
            {
                ExecutionRole = props.ExecutionRole,
                TaskRole = taskRole,
                Family = $"invoice-app-identity-api-migration-{config.Environment}"
            });

        migrationTaskDef.AddContainer($"invoice-app-identity-api-migration-{config.Environment}",
            new ContainerDefinitionOptions
            {
                Image = ContainerImage.FromEcrRepository(migrationsRepo, config.MigrationTag),
                Essential = true,
                Secrets = new Dictionary<string, Secret>
                {
                    ["DB_HOST"] = Secret.FromSecretsManager(props.DatabaseSecret!, "host"),
                    ["DB_PORT"] = Secret.FromSecretsManager(props.DatabaseSecret!, "port"),
                    ["DB_USER"] = Secret.FromSecretsManager(props.DatabaseSecret!, "username"),
                    ["DB_PASS"] = Secret.FromSecretsManager(props.DatabaseSecret!, "password"),
                    ["DB_NAME"] = Secret.FromSecretsManager(props.DatabaseSecret!, "dbInstanceIdentifier")
                },
                Logging = LogDrivers.AwsLogs(new AwsLogDriverProps
                {
                    StreamPrefix = $"invoice-app-identity-api-migrations-{config.Environment}",
                    Mode = AwsLogDriverMode.NON_BLOCKING,
                    MaxBufferSize = Size.Mebibytes(25)
                })
            });

        var targetGroup = new ApplicationTargetGroup(this, "invoice-app-identity-api-target-group",
            new ApplicationTargetGroupProps
            {
                Port = 8080,
                Targets = [service],
                HealthCheck = new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck
                {
                    Port = "8080",
                    Path = "/healthz",
                    HealthyHttpCodes = "200-299"
                },
                Vpc = props.Vpc
            });

        var listener = loadBalancer.AddListener("invoice-app-listener", new ApplicationListenerProps
        {
            Port = 80,
            Open = true
        });

        listener.AddTargetGroups("ECS", new AddApplicationTargetGroupsProps
        {
            TargetGroups = [targetGroup]
        });
    }
}

public sealed record IdentityApiProps
{
    public required IVpc Vpc { get; init; }
    public required ISecurityGroup DatabaseSecurityGroup { get; init; }
    public required ISecret? DatabaseSecret { get; init; }
    public required IRole ExecutionRole { get; init; }
    public required ICluster Cluster { get; init; }
}
