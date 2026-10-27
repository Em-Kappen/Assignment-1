using company_mgmt.Services;
using Microsoft.AspNetCore.Mvc;

namespace company_mgmt.Controllers;

[ApiController]
[Route("api/performance")]
public sealed class PerformanceController(
    IDatabaseSetupService setupService,
    IPerformanceService performanceService) : ControllerBase
{
    [HttpGet("table-counts")]
    public async Task<ActionResult<Dictionary<string, long>>> GetTableCounts(
        CancellationToken cancellationToken) =>
        Ok(await setupService.GetTableCountsAsync(cancellationToken));

    [HttpGet("database-summary")]
    public async Task<ActionResult<DatabaseSummary>> GetDatabaseSummary(
        CancellationToken cancellationToken) =>
        Ok(await setupService.GetDatabaseSummaryAsync(cancellationToken));

    [HttpGet("index-summary")]
    public async Task<ActionResult<IReadOnlyList<IndexSummary>>> GetIndexSummary(
        CancellationToken cancellationToken) =>
        Ok(await setupService.GetIndexSummaryAsync(cancellationToken));

    [HttpPost("benchmarks")]
    public async Task<ActionResult<BenchmarkSuiteResult>> RunBenchmarks(
        CancellationToken cancellationToken) =>
        Ok(await performanceService.RunBenchmarksAsync(
            $"{Request.Scheme}://{Request.Host}/", cancellationToken));

    [HttpGet("benchmarks")]
    public async Task<ActionResult<IReadOnlyList<BenchmarkHistoryItem>>> GetBenchmarkHistory(
        CancellationToken cancellationToken) =>
        Ok(await performanceService.GetHistoryAsync(cancellationToken));
}