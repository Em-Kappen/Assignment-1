using company_mgmt.Services;
using Microsoft.AspNetCore.Mvc;

namespace company_mgmt.Controllers;

[ApiController]
[Route("api/performance")]
public sealed class PerformanceController(
    IDatabaseSetupService setupService) : ControllerBase
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
    public IActionResult RunBenchmarks() =>
        StatusCode(StatusCodes.Status410Gone, new ProblemDetails
        {
            Status = StatusCodes.Status410Gone,
            Title = "Benchmarks are disabled",
            Detail = "Benchmark persistence is outside the SQL_main Customer, Order, and Product integration."
        });

    [HttpGet("benchmarks")]
    public IActionResult GetBenchmarkHistory() =>
        StatusCode(StatusCodes.Status410Gone, new ProblemDetails
        {
            Status = StatusCodes.Status410Gone,
            Title = "Benchmark history is unavailable",
            Detail = "SQL_main benchmark history is not part of this integration."
        });
}