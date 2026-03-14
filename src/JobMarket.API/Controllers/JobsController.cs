using JobMarket.Application.Features.Jobs.Queries.GetJobById;
using JobMarket.Application.Features.Jobs.Queries.GetRelatedJobs;
using JobMarket.Application.Features.Jobs.Queries.GetTrendingJobs;
using JobMarket.Application.Features.Jobs.Queries.SearchJobs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMarket.API.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Search jobs with filters and full-text or vector similarity</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SearchJobsQuery query, CancellationToken ct)
        => Ok(await _mediator.Send(query, ct));

    /// <summary>Get full job details by ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetJobByIdQuery(id), ct));

    /// <summary>Get semantically similar jobs based on a source job's embedding</summary>
    [HttpGet("{id:guid}/related")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelated(Guid id, [FromQuery] int topK = 10, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetRelatedJobsQuery(id, topK), ct));

    /// <summary>Get trending jobs ordered by recency and demand signals</summary>
    [HttpGet("trending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrending([FromQuery] int limit = 20, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetTrendingJobsQuery(limit), ct));
}
