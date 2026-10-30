namespace company_mgmt.Services;

public interface IApiBenchmarkService
{
    Task<ApiBenchmarkReport> RunAsync(CancellationToken cancellationToken);
}

/// <summary>Summary of one bounded API benchmark run.</summary>
public sealed record ApiBenchmarkReport(
    DateTimeOffset StartedAtUtc,
    double DurationMilliseconds,
    string TargetBaseUrl,
    int IterationsPerScenario,
    int ConcurrentScenarios,
    IReadOnlyList<ApiBenchmarkScenarioResult> Scenarios);

/// <summary>Latency, payload, throughput, and status metrics for one API scenario.</summary>
public sealed record ApiBenchmarkScenarioResult(
    string Name,
    string RequestPath,
    int Requests,
    int SuccessfulRequests,
    int FailedRequests,
    IReadOnlyDictionary<string, int> StatusCodes,
    double RequestsPerSecond,
    long TotalResponsePayloadBytes,
    double AverageResponsePayloadBytes,
    double MinimumLatencyMilliseconds,
    double MeanLatencyMilliseconds,
    double P50LatencyMilliseconds,
    double P95LatencyMilliseconds,
    double MaximumLatencyMilliseconds,
    IReadOnlyList<string> Errors);
