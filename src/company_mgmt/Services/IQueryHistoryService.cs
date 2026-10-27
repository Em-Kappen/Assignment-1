namespace company_mgmt.Services;

public interface IQueryHistoryService
{
    Task<IReadOnlyList<QueryHistoryResponse>> GetRecentAsync(CancellationToken cancellationToken);
    Task<QueryHistoryResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<QueryHistoryResponse> CreateAsync(CreateQueryHistoryRequest request, CancellationToken cancellationToken);
}

/// <summary>Query execution metadata returned to the browser.</summary>
public sealed record QueryHistoryResponse(
    long Id,
    DateTimeOffset ExecutedAt,
    string EntitySet,
    string RequestUrl,
    double DurationMilliseconds,
    long PayloadBytes,
    int HttpStatusCode,
    int ResultCount,
    string? ErrorMessage);

/// <summary>Query execution metadata sent by the browser.</summary>
public sealed record CreateQueryHistoryRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(40)]
    public required string EntitySet { get; init; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(2048)]
    public required string RequestUrl { get; init; }

    [System.ComponentModel.DataAnnotations.Range(0, 3600000)]
    public double DurationMilliseconds { get; init; }

    [System.ComponentModel.DataAnnotations.Range(0, long.MaxValue)]
    public long PayloadBytes { get; init; }

    [System.ComponentModel.DataAnnotations.Range(0, 599)]
    public int HttpStatusCode { get; init; }

    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int ResultCount { get; init; }

    [System.ComponentModel.DataAnnotations.MaxLength(2000)]
    public string? ErrorMessage { get; init; }
}
