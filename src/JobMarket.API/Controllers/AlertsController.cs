using JobMarket.API.Extensions;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMarket.API.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AlertsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <summary>Get all job alerts for the authenticated user</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        Guid userId = User.GetUserId();
        var alerts = await _unitOfWork.JobAlerts.FindAsync(a => a.UserId == userId, ct);
        return Ok(alerts);
    }

    /// <summary>Create a new job alert with keyword and location filters</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAlertRequest request, CancellationToken ct)
    {
        Guid userId = User.GetUserId();
        JobAlert alert = JobAlert.Create(userId, request.Keywords);

        await _unitOfWork.JobAlerts.AddAsync(alert, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), new { }, alert.Id);
    }

    /// <summary>Delete an existing job alert</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        Guid userId = User.GetUserId();
        var alerts = await _unitOfWork.JobAlerts.FindAsync(a => a.Id == id && a.UserId == userId, ct);
        var alert = alerts.FirstOrDefault();

        if (alert == null)
            return NotFound();

        await _unitOfWork.JobAlerts.DeleteAsync(alert, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}

public record CreateAlertRequest(List<string> Keywords);
