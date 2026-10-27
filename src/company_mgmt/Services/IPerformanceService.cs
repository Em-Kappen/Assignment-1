namespace company_mgmt.Services;

public interface IPerformanceService
{
    Task<BenchmarkSuiteResult> RunBenchmarksAsync(string baseAddress, CancellationToken cancellationToken);
    Task<IReadOnlyList<BenchmarkHistoryItem>> GetHistoryAsync(CancellationToken cancellationToken);
}