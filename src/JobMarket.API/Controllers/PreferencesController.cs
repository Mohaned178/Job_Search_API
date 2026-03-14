using JobMarket.API.Extensions;
using JobMarket.Application.Features.Preferences.Commands.UpdatePreferences;
using JobMarket.Application.Features.Preferences.Queries.GetPersonalizedFeed;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMarket.API.Controllers;

[ApiController]
[Route("api/preferences")]
[Authorize]
public class PreferencesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PreferencesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Update job preference settings (categories, locations, salary, seniority)</summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] UpdatePreferencesCommand command, CancellationToken ct)
    {
        UpdatePreferencesCommand withUser = command with { UserId = User.GetUserId() };
        await _mediator.Send(withUser, ct);
        return NoContent();
    }

    /// <summary>Get a personalized job feed based on saved preferences</summary>
    [HttpGet("feed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetPersonalizedFeedQuery(User.GetUserId(), page, pageSize), ct));
}
