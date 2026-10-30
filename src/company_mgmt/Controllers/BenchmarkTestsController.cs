using company_mgmt.Services;
using Microsoft.AspNetCore.Mvc;

namespace company_mgmt.Controllers;

[ApiController]
[Route("benchmark-tests")]
public sealed class BenchmarkTestsController(
    IApiBenchmarkService benchmarkService) : ControllerBase
{
    /// <summary>Runs a bounded read-only benchmark suite against the configured OData API.</summary>
    [HttpGet]
    [ProducesResponseType<ApiBenchmarkReport>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiBenchmarkReport>> Run(CancellationToken cancellationToken) =>
        Ok(await benchmarkService.RunAsync(cancellationToken));
}
