using Microsoft.AspNetCore.Mvc;

namespace company_mgmt.Controllers;

[ApiController]
[Route("api/setup")]
public sealed class SetupController : ControllerBase
{
    [HttpPost("schema")]
    public IActionResult RunSchema() =>
        StatusCode(StatusCodes.Status410Gone, new ProblemDetails
        {
            Status = StatusCodes.Status410Gone,
            Title = "Schema setup is disabled",
            Detail = "SQL_main is an existing database; this application does not apply schema changes."
        });

    [HttpPost("seed")]
    public IActionResult RunSeed() =>
        StatusCode(StatusCodes.Status410Gone, new ProblemDetails
        {
            Status = StatusCodes.Status410Gone,
            Title = "Data seeding is disabled",
            Detail = "Synthetic seed data is not inserted into SQL_main."
        });
}