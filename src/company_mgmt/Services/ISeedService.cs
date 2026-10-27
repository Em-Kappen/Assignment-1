namespace company_mgmt.Services;

public interface ISeedService
{
    Task<SetupExecutionResult> RunSeedAsync(CancellationToken cancellationToken);
}