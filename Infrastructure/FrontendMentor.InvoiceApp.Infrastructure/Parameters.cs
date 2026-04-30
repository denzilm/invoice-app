namespace FrontendMentor.InvoiceApp.Infrastructure;

public sealed record Parameters
{
    private readonly string _parameterPrefix;

    public Parameters(AppConfig config)
    {
        _parameterPrefix = $"/{AppConfig.AppName}/{config.Environment}";
    }

    public string Network => $"{_parameterPrefix}/network";
    public string VpcId => $"{Network}/vpc-id";
    public string EcrIdentityRepositoryName => $"{_parameterPrefix}/identity/repository-name";
    public string EcrIdentityMigrationRepositoryName => $"{_parameterPrefix}/identity/migration/repository-name";
    public string ClusterArn => $"{_parameterPrefix}/cluster-arn";
    public string ExecutionRoleArn => $"{_parameterPrefix}/execution-role-arn";
}
