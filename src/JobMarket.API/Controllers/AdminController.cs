using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMarket.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IJobIngestionJob _ingestionJob;

    public AdminController(IJobIngestionJob ingestionJob) => _ingestionJob = ingestionJob;

    /// <summary>Manually trigger the job ingestion pipeline across all sources</summary>
    [HttpPost("ingest")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult TriggerIngestion()
    {
        Hangfire.BackgroundJob.Enqueue<IJobIngestionJob>(j => j.IngestAsync(CancellationToken.None));
        return Accepted(new { message = "Ingestion job queued." });
    }
}
