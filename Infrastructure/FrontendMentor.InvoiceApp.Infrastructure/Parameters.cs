namespace FrontendMentor.InvoiceApp.Infrastructure;

public sealed record Parameters
{
    private readonly string _globalPrefix;

    public Parameters(AppConfig config)
    {
        Network = $"/{AppConfig.AppName}/{config.Environment}";
        _globalPrefix = $"/{AppConfig.AppName}";
    }

    public string Network => $"{field}/network";
    public string VpcId => $"{Network}/vpc-id";
    public string GetEcrRepositoryName(string repository) => $"{_globalPrefix}/{repository}/repository-name";
    public string GetEcrMigrationRepositoryName(string repository) => $"{_globalPrefix}/{repository}/migration/repository-name";
    public string ClusterArn => $"{_globalPrefix}/cluster-arn";
    public string ExecutionRoleArn => $"{_globalPrefix}/execution-role-arn";
}
