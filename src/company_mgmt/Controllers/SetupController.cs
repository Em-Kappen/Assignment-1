using company_mgmt.Services;
using Microsoft.AspNetCore.Mvc;

namespace company_mgmt.Controllers;

[ApiController]
[Route("api/setup")]
public sealed class SetupController(
    IDatabaseSetupService setupService,
    ISeedService seedService) : ControllerBase
{
    [HttpPost("schema")]
    public async Task<ActionResult<SetupExecutionResult>> RunSchema(CancellationToken cancellationToken) =>
        Ok(await setupService.RunSchemaAsync(cancellationToken));

    [HttpPost("seed")]
    public async Task<ActionResult<SetupExecutionResult>> RunSeed(CancellationToken cancellationToken) =>
        Ok(await seedService.RunSeedAsync(cancellationToken));
}