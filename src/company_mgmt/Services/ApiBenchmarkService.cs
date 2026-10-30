using System.Collections.Concurrent;
using System.Diagnostics;

namespace company_mgmt.Services;

public sealed class ApiBenchmarkService(
    IHttpClientFactory httpClientFactory) : IApiBenchmarkService
{
    private const int IterationsPerScenario = 10;
    private const int ConcurrentScenarios = 4;

    private static readonly BenchmarkScenario[] Scenarios =
    [
        new(
            "Customer projection",
            "/odata/Customers?$select=CustomerId,Name,City,State,CountryCode&$top=100"),
        new(
            "Product projection",
            "/odata/Products?$select=ProductId,Description,CatalogNumber,ProductStatusId&$top=100"),
        new(
            "Order line and product expansion",
            "/odata/Orders?$select=SalesOrderGenerationId,OrderDate,StatusCode,SalesTotal&$expand=OrderItems($top=5;$select=LineId,ShipQty,Price;$expand=Product($select=ProductId,Description,CatalogNumber))&$top=50"),
        new(
            "Customer profile and nested order history",
            "/odata/Customers?$select=CustomerId,Name,City,State&$expand=Orders($top=3;$select=SalesOrderGenerationId,OrderDate,StatusCode,SalesTotal;$expand=OrderItems($top=3;$select=LineId,ShipQty,Price;$expand=Product($select=ProductId,Description,CatalogNumber)))&$top=10"),
        new(
            "Order item projection",
            "/odata/OrderItems?$select=SalesOrderLineGenerationId,LineId,ShipQty,Price&$expand=Product($select=ProductId,Description,CatalogNumber)&$top=100")
    ];

    public async Task<ApiBenchmarkReport> RunAsync(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("BenchmarkTarget");
        var startedAt = DateTimeOffset.UtcNow;
        var totalTimer = Stopwatch.StartNew();
        var results = new ApiBenchmarkScenarioResult[Scenarios.Length];

        await Parallel.ForEachAsync(
            Enumerable.Range(0, Scenarios.Length),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = ConcurrentScenarios,
                CancellationToken = cancellationToken
            },
            async (index, token) =>
            {
                results[index] = await RunScenarioAsync(Scenarios[index], client, token);
            });

        totalTimer.Stop();
        return new ApiBenchmarkReport(
            startedAt,
            totalTimer.Elapsed.TotalMilliseconds,
            client.BaseAddress!.ToString(),
            IterationsPerScenario,
            ConcurrentScenarios,
            results);
    }

    private static async Task<ApiBenchmarkScenarioResult> RunScenarioAsync(
        BenchmarkScenario scenario,
        HttpClient client,
        CancellationToken cancellationToken)
    {
        var latencies = new List<double>(IterationsPerScenario);
        var statusCodes = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        var errors = new ConcurrentQueue<string>();
        long responseBytes = 0;
        var successfulRequests = 0;
        var scenarioTimer = Stopwatch.StartNew();

        for (var iteration = 0; iteration < IterationsPerScenario; iteration++)
        {
            var requestTimer = Stopwatch.StartNew();
            try
            {
                using var response = await client.GetAsync(
                    scenario.RequestPath,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                var payload = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                requestTimer.Stop();
                latencies.Add(requestTimer.Elapsed.TotalMilliseconds);
                Interlocked.Add(ref responseBytes, payload.LongLength);

                var status = ((int)response.StatusCode).ToString(System.Globalization.CultureInfo.InvariantCulture);
                statusCodes.AddOrUpdate(status, 1, static (_, count) => count + 1);
                if (response.IsSuccessStatusCode)
                    Interlocked.Increment(ref successfulRequests);
                else
                    errors.Enqueue($"HTTP {status} {response.ReasonPhrase}");
            }
            catch (HttpRequestException exception)
            {
                requestTimer.Stop();
                latencies.Add(requestTimer.Elapsed.TotalMilliseconds);
                statusCodes.AddOrUpdate("ERROR", 1, static (_, count) => count + 1);
                errors.Enqueue(exception.Message);
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                requestTimer.Stop();
                latencies.Add(requestTimer.Elapsed.TotalMilliseconds);
                statusCodes.AddOrUpdate("TIMEOUT", 1, static (_, count) => count + 1);
                errors.Enqueue(exception.Message);
            }
        }

        scenarioTimer.Stop();
        var orderedLatencies = latencies.Order().ToArray();
        return new ApiBenchmarkScenarioResult(
            scenario.Name,
            scenario.RequestPath,
            IterationsPerScenario,
            successfulRequests,
            IterationsPerScenario - successfulRequests,
            new SortedDictionary<string, int>(statusCodes, StringComparer.Ordinal),
            IterationsPerScenario / scenarioTimer.Elapsed.TotalSeconds,
            responseBytes,
            (double)responseBytes / IterationsPerScenario,
            orderedLatencies[0],
            orderedLatencies.Average(),
            Percentile(orderedLatencies, 0.50),
            Percentile(orderedLatencies, 0.95),
            orderedLatencies[^1],
            errors.ToArray());
    }

    private static double Percentile(IReadOnlyList<double> sortedValues, double percentile)
    {
        var index = Math.Clamp((int)Math.Ceiling(sortedValues.Count * percentile) - 1, 0, sortedValues.Count - 1);
        return sortedValues[index];
    }

    private sealed record BenchmarkScenario(string Name, string RequestPath);
}
