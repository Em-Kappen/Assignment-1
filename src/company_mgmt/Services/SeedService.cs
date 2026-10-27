namespace company_mgmt.Services;

public sealed class SeedService(IDatabaseSetupService setupService) : ISeedService
{
    public Task<SetupExecutionResult> RunSeedAsync(CancellationToken cancellationToken) =>
        setupService.RunSeedAsync(cancellationToken);
}