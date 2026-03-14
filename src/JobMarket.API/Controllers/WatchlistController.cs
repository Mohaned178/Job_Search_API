using JobMarket.API.Extensions;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMarket.API.Controllers;

[ApiController]
[Route("api/watchlist")]
[Authorize]
public class WatchlistController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public WatchlistController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <summary>Get all watchlisted jobs for the authenticated user</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        Guid userId = User.GetUserId();
        var items = await _unitOfWork.WatchlistItems.FindAsync(w => w.UserId == userId, ct);
        return Ok(items);
    }

    /// <summary>Add a job to the user's watchlist</summary>
    [HttpPost("{jobId:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(Guid jobId, CancellationToken ct)
    {
        Guid userId = User.GetUserId();
        bool exists = await _unitOfWork.WatchlistItems.ExistsAsync(
            w => w.UserId == userId && w.JobId == jobId, ct);

        if (exists)
            return Conflict("Job is already in watchlist.");

        WatchlistItem item = WatchlistItem.Create(userId, jobId);
        await _unitOfWork.WatchlistItems.AddAsync(item, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), new { }, item.Id);
    }

    /// <summary>Remove a job from the user's watchlist</summary>
    [HttpDelete("{jobId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid jobId, CancellationToken ct)
    {
        Guid userId = User.GetUserId();
        var items = await _unitOfWork.WatchlistItems.FindAsync(
            w => w.UserId == userId && w.JobId == jobId, ct);

        var item = items.FirstOrDefault();
        if (item == null)
            return NotFound();

        await _unitOfWork.WatchlistItems.DeleteAsync(item, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
