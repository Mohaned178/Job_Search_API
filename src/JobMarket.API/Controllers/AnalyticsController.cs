using JobMarket.Application.Features.Analytics.Queries.GetSalaryIntelligence;
using JobMarket.Application.Features.Analytics.Queries.GetTrendingSkills;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobMarket.API.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get trending technical skills ranked by job market demand</summary>
    [HttpGet("trending-skills")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrendingSkills(
        [FromQuery] int limit = 20,
        [FromQuery] string? country = null,
        CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetTrendingSkillsQuery(limit, country), ct));

    /// <summary>Get salary intelligence for a given job title and location</summary>
    [HttpGet("salary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalaryIntelligence(
        [FromQuery] string jobTitle,
        [FromQuery] string? country,
        CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetSalaryIntelligenceQuery(jobTitle, country), ct));
}
