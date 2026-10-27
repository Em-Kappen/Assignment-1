namespace company_mgmt.Models;

public class QueryHistory
{
    public long QueryHistoryId { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
    public string EntitySet { get; set; } = "";
    public string RequestUrl { get; set; } = "";
    public double DurationMilliseconds { get; set; }
    public long PayloadBytes { get; set; }
    public int HttpStatusCode { get; set; }
    public int ResultCount { get; set; }
    public string? ErrorMessage { get; set; }
}
