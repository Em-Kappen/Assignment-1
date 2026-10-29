using Microsoft.AspNetCore.Mvc;

namespace company_mgmt.Controllers;

[ApiController]
[Route("api/query-history")]
public sealed class QueryHistoryController : ControllerBase
{
    [HttpGet]
    public IActionResult GetRecent() =>
        StatusCode(StatusCodes.Status410Gone, new ProblemDetails
        {
            Status = StatusCodes.Status410Gone,
            Title = "Query history is unavailable",
            Detail = "Query-history persistence is outside the SQL_main Customer, Order, and Product integration."
        });

    [HttpGet("{id:long}")]
    public IActionResult GetById(long id) => GetRecent();

    [HttpPost]
    public IActionResult Create() => GetRecent();
}
