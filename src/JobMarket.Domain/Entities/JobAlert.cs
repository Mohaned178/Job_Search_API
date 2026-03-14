using JobMarket.Domain.Common;

namespace JobMarket.Domain.Entities;

public class JobAlert : AuditableEntity
{
    public Guid UserId { get; private set; }
    public List<string> Keywords { get; private set; } = new();
    public bool IsActive { get; private set; }
    public DateTime? LastTriggeredAt { get; private set; }

    private JobAlert() { }

    public static JobAlert Create(Guid userId, List<string> keywords)
        => new JobAlert
        {
            UserId = userId,
            Keywords = keywords,
            IsActive = true
        };

    public void Trigger()
    {
        LastTriggeredAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
