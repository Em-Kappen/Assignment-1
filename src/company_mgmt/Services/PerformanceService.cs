using System.Diagnostics;
using System.Net;
using System.Text.Json;
using company_mgmt.Data;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Services;

public sealed class PerformanceService(
    IHttpClientFactory httpClientFactory,
    AppDbContext context,
    IDatabaseSetupService setupService,
    ILogger<PerformanceService> logger) : IPerformanceService
{
    private static readonly (string Name, string Endpoint)[] Scenarios =
    [
        ("Customers top 100", "/odata/Customers?$top=100"),
        ("Customers top 1000", "/odata/Customers?$top=1000"),
        ("Electronics products", "/odata/Products?$filter=Category%20eq%20%27Electronics%27"),
        ("IT employees", "/odata/Employees?$filter=Department%20eq%20%27IT%27"),
        ("Top-paid employees", "/odata/Employees?$orderby=Salary%20desc&$top=100"),
        ("Orders with customer", "/odata/Orders?$expand=Customer"),
        ("Orders with items", "/odata/Orders?$expand=OrderItems"),
        ("Orders with customer and items", "/odata/Orders?$expand=Customer,OrderItems"),
        ("Orders with count", "/odata/Orders?$count=true"),
        ("Customers with orders", "/odata/Customers?$expand=Orders")
    ];

    public async Task<BenchmarkSuiteResult> RunBenchmarksAsync(
        string baseAddress,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Benchmarks");
        var counts = await setupService.GetTableCountsAsync(cancellationToken);
        var dataset = counts
            .Where(pair => pair.Key is not "branches" and not "benchmark_runs" and not "query_history")
            .Sum(pair => pair.Value);
        var results = new List<BenchmarkScenarioResult>(Scenarios.Length);
        foreach (var scenario in Scenarios)
        {
            var durations = new List<double>(5);
            var statuses = new List<int>(5);
            var successes = 0;
            long payload = 0;
            for (var run = 0; run < 5; run++)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    using var response = await client.GetAsync(
                        new Uri(new Uri(baseAddress), scenario.Endpoint),
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);
                    await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var buffer = new byte[64 * 1024];
                    long bytesRead = 0;
                    int read;
                    while ((read = await body.ReadAsync(buffer, cancellationToken)) > 0)
                        bytesRead += read;
                    stopwatch.Stop();
                    durations.Add(stopwatch.Elapsed.TotalMilliseconds);
                    statuses.Add((int)response.StatusCode);
                    payload = bytesRead;
                    if (response.IsSuccessStatusCode)
                        successes++;
                }
                catch (HttpRequestException exception)
                {
                    stopwatch.Stop();
                    durations.Add(stopwatch.Elapsed.TotalMilliseconds);
                    statuses.Add((int)(exception.StatusCode ?? HttpStatusCode.ServiceUnavailable));
                    logger.LogWarning(exception, "Benchmark request failed for {Endpoint}", scenario.Endpoint);
                }
                catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
                {
                    stopwatch.Stop();
                    durations.Add(stopwatch.Elapsed.TotalMilliseconds);
                    statuses.Add((int)HttpStatusCode.RequestTimeout);
                    logger.LogWarning(exception, "Benchmark request timed out for {Endpoint}", scenario.Endpoint);
                }
                catch (IOException exception)
                {
                    stopwatch.Stop();
                    durations.Add(stopwatch.Elapsed.TotalMilliseconds);
                    statuses.Add((int)HttpStatusCode.BadGateway);
                    logger.LogWarning(exception, "Benchmark response read failed for {Endpoint}", scenario.Endpoint);
                }
            }

            var ordered = durations.Order().ToArray();
            var median = ordered.Length % 2 == 0
                ? (ordered[ordered.Length / 2 - 1] + ordered[ordered.Length / 2]) / 2
                : ordered[ordered.Length / 2];
            var result = new BenchmarkScenarioResult(
                scenario.Name,
                scenario.Endpoint,
                durations.Min(),
                durations.Max(),
                durations.Average(),
                median,
                successes,
                5 - successes,
                payload,
                statuses);
            results.Add(result);
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO dbo.benchmark_runs
                 (scenario, endpoint, executed_at, dataset_size, min_duration_ms, max_duration_ms,
                  average_duration_ms, median_duration_ms, success_count, failure_count, payload_bytes, status_codes)
                 VALUES ({result.Scenario}, {result.Endpoint}, {DateTimeOffset.UtcNow}, {dataset},
                         {result.MinimumMilliseconds}, {result.MaximumMilliseconds},
                         {result.AverageMilliseconds}, {result.MedianMilliseconds},
                         {result.SuccessCount}, {result.FailureCount}, {result.PayloadBytes},
                         {JsonSerializer.Serialize(statuses)})
                 """,
                cancellationToken);
            logger.LogInformation(
                "Benchmark {Scenario} completed: {SuccessCount}/5 succeeded; average {AverageMs} ms",
                scenario.Name, successes, result.AverageMilliseconds);
        }

        return new BenchmarkSuiteResult(DateTimeOffset.UtcNow, dataset, results);
    }

    public async Task<IReadOnlyList<BenchmarkHistoryItem>> GetHistoryAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Database.SqlQuery<BenchmarkHistoryItem>(
                $"""
                 SELECT TOP (200) benchmark_run_id AS [Id], scenario AS [Scenario], endpoint AS [Endpoint],
                        executed_at AS [ExecutedAt], dataset_size AS [DatasetSize],
                        min_duration_ms AS [MinimumMilliseconds], max_duration_ms AS [MaximumMilliseconds],
                        average_duration_ms AS [AverageMilliseconds], median_duration_ms AS [MedianMilliseconds],
                        success_count AS [SuccessCount], failure_count AS [FailureCount], payload_bytes AS [PayloadBytes],
                        status_codes AS [StatusCodesJson]
                 FROM dbo.benchmark_runs
                 ORDER BY executed_at DESC
                 """)
            .ToListAsync(cancellationToken);
        return rows;
    }
}

public sealed record BenchmarkSuiteResult(
    DateTimeOffset ExecutedAt,
    long DatasetSize,
    IReadOnlyList<BenchmarkScenarioResult> Scenarios);

public sealed record BenchmarkScenarioResult(
    string Scenario,
    string Endpoint,
    double MinimumMilliseconds,
    double MaximumMilliseconds,
    double AverageMilliseconds,
    double MedianMilliseconds,
    int SuccessCount,
    int FailureCount,
    long PayloadBytes,
    IReadOnlyList<int> StatusCodes);

public sealed record BenchmarkHistoryItem(
    long Id,
    string Scenario,
    string Endpoint,
    DateTimeOffset ExecutedAt,
    long DatasetSize,
    double MinimumMilliseconds,
    double MaximumMilliseconds,
    double AverageMilliseconds,
    double MedianMilliseconds,
    int SuccessCount,
    int FailureCount,
    long PayloadBytes,
    string StatusCodesJson);