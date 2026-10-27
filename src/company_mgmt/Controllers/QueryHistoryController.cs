using company_mgmt.Services;
using Microsoft.AspNetCore.Mvc;

namespace company_mgmt.Controllers;

[ApiController]
[Route("api/query-history")]
public sealed class QueryHistoryController(IQueryHistoryService historyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QueryHistoryResponse>>> GetRecent(
        CancellationToken cancellationToken) =>
        Ok(await historyService.GetRecentAsync(cancellationToken));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<QueryHistoryResponse>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var entry = await historyService.GetByIdAsync(id, cancellationToken);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    public async Task<ActionResult<QueryHistoryResponse>> Create(
        CreateQueryHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var entry = await historyService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }
}
