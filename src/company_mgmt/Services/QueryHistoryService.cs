using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Services;

public sealed class QueryHistoryService(AppDbContext context) : IQueryHistoryService
{
    public async Task<IReadOnlyList<QueryHistoryResponse>> GetRecentAsync(CancellationToken cancellationToken) =>
        await context.QueryHistory.AsNoTracking()
            .OrderByDescending(entry => entry.ExecutedAt)
            .Take(100)
            .Select(entry => new QueryHistoryResponse(
                entry.QueryHistoryId,
                entry.ExecutedAt,
                entry.EntitySet,
                entry.RequestUrl,
                entry.DurationMilliseconds,
                entry.PayloadBytes,
                entry.HttpStatusCode,
                entry.ResultCount,
                entry.ErrorMessage))
            .ToListAsync(cancellationToken);

    public async Task<QueryHistoryResponse?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var entry = await context.QueryHistory.AsNoTracking()
            .Where(item => item.QueryHistoryId == id)
            .Select(item => new QueryHistoryResponse(
                item.QueryHistoryId,
                item.ExecutedAt,
                item.EntitySet,
                item.RequestUrl,
                item.DurationMilliseconds,
                item.PayloadBytes,
                item.HttpStatusCode,
                item.ResultCount,
                item.ErrorMessage))
            .SingleOrDefaultAsync(cancellationToken);
        return entry;
    }

    public async Task<QueryHistoryResponse> CreateAsync(
        CreateQueryHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var entry = new QueryHistory
        {
            ExecutedAt = DateTimeOffset.UtcNow,
            EntitySet = request.EntitySet,
            RequestUrl = request.RequestUrl,
            DurationMilliseconds = request.DurationMilliseconds,
            PayloadBytes = request.PayloadBytes,
            HttpStatusCode = request.HttpStatusCode,
            ResultCount = request.ResultCount,
            ErrorMessage = request.ErrorMessage
        };
        context.QueryHistory.Add(entry);
        await context.SaveChangesAsync(cancellationToken);
        return new QueryHistoryResponse(
            entry.QueryHistoryId,
            entry.ExecutedAt,
            entry.EntitySet,
            entry.RequestUrl,
            entry.DurationMilliseconds,
            entry.PayloadBytes,
            entry.HttpStatusCode,
            entry.ResultCount,
            entry.ErrorMessage);
    }
}
