using JobMarket.Domain.Common;

namespace JobMarket.Domain.Entities;

public class WatchlistItem : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid JobId { get; private set; }
    public DateTime SavedAt { get; private set; }

    private WatchlistItem() { }

    public static WatchlistItem Create(Guid userId, Guid jobId)
        => new WatchlistItem
        {
            UserId = userId,
            JobId = jobId,
            SavedAt = DateTime.UtcNow
        };
}
