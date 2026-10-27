namespace company_mgmt.Services;

public interface IDatabaseSetupService
{
    Task<SetupExecutionResult> RunSchemaAsync(CancellationToken cancellationToken);
    Task<SetupExecutionResult> RunSeedAsync(CancellationToken cancellationToken);
    Task<Dictionary<string, long>> GetTableCountsAsync(CancellationToken cancellationToken);
    Task<DatabaseSummary> GetDatabaseSummaryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<IndexSummary>> GetIndexSummaryAsync(CancellationToken cancellationToken);
}