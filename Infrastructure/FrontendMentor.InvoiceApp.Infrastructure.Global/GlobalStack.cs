using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace FrontendMentor.InvoiceApp.Infrastructure.Global;

public sealed class GlobalStack : Stack
{
    private static readonly string[] EcrRepositories = ["invoice-app-identity-api"];

    internal GlobalStack(Construct scope, string id, AppConfig config, IStackProps props) : base(scope, id, props)
    {
        var parameters = new Parameters(config);
        foreach (var repositoryName in EcrRepositories)
        {
            CreateEcrRepository(this, repositoryName, repositoryName, parameters.GetEcrRepositoryName(repositoryName));
            CreateEcrRepository(this, $"{repositoryName}-migration", $"{repositoryName}-migration", parameters.GetEcrMigrationRepositoryName($"{repositoryName}-migration"));
        }

        var executionRole = new Role(this, "invoice-app-execution-role", new RoleProps
        {
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
        });

        executionRole.AddManagedPolicy(
            ManagedPolicy
                .FromManagedPolicyArn(
                    this, "TaskExecutionPolicy", "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"));

        _ = new StringParameter(this, "ExecutionRole", new StringParameterProps
        {
            ParameterName = parameters.ExecutionRoleArn,
            StringValue = executionRole.RoleArn
        });
    }

    private static void CreateEcrRepository(Construct scope, string id, string repositoryName, string parameterName)
    {
        var repository = new Repository(scope, id, new RepositoryProps
        {
            RepositoryName = repositoryName,
            ImageTagMutability = TagMutability.MUTABLE,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        _ = new StringParameter(scope, $"{repositoryName}EcrRepository", new StringParameterProps
        {
            ParameterName = parameterName,
            StringValue = repository.RepositoryName
        });
    }
}
