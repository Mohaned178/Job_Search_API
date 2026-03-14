using JobMarket.API.Extensions;
using JobMarket.Application.Features.CVs.Commands.DeleteCv;
using JobMarket.Application.Features.CVs.Commands.UploadCv;
using JobMarket.Application.Features.CVs.Queries.GetAtsScore;
using JobMarket.Application.Features.CVs.Queries.GetCvStatus;
using JobMarket.Application.Features.CVs.Queries.GetGapAnalysis;
using JobMarket.Application.Features.CVs.Queries.GetJobMatches;
using JobMarket.Application.Features.CVs.Queries.GetRoadmap;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMarket.API.Controllers;

[ApiController]
[Route("api/cvs")]
[Authorize]
public class CvController : ControllerBase
{
    private readonly IMediator _mediator;

    public CvController(IMediator mediator) => _mediator = mediator;

    /// <summary>Upload a CV file (PDF or DOCX) for processing</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        Guid userId = User.GetUserId();
        UploadCvCommand command = new(userId, file.OpenReadStream(), file.FileName);
        return Ok(await _mediator.Send(command, ct));
    }

    /// <summary>Get CV processing status</summary>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetCvStatusQuery(id, User.GetUserId()), ct));

    /// <summary>Get ATS score and improvement suggestions for a CV</summary>
    [HttpGet("{id:guid}/ats-score")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAtsScore(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetAtsScoreQuery(id, User.GetUserId()), ct));

    /// <summary>Get semantically similar job matches for a CV</summary>
    [HttpGet("{id:guid}/matches")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMatches(Guid id, [FromQuery] int topK = 10, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetJobMatchesQuery(id, User.GetUserId(), topK), ct));

    /// <summary>Get skill gap analysis between CV and a target job</summary>
    [HttpGet("{id:guid}/gap-analysis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGapAnalysis(Guid id, [FromQuery] string targetRole, CancellationToken ct)
        => Ok(await _mediator.Send(new GetGapAnalysisQuery(id, User.GetUserId(), targetRole), ct));

    /// <summary>Get a personalized learning roadmap to close skill gaps</summary>
    [HttpGet("{id:guid}/roadmap")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoadmap(Guid id, [FromQuery] string targetRole, CancellationToken ct)
        => Ok(await _mediator.Send(new GetRoadmapQuery(id, User.GetUserId(), targetRole), ct));

    /// <summary>Delete a CV and its associated storage file</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCvCommand(id, User.GetUserId()), ct);
        return NoContent();
    }
}
