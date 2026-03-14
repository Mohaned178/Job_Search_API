using JobMarket.Domain.Entities;

namespace JobMarket.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Job> Jobs { get; }
    IRepository<CV> CVs { get; }
    IRepository<User> Users { get; }
    IRepository<UserPreferences> UserPreferences { get; }
    IRepository<JobAlert> JobAlerts { get; }
    IRepository<WatchlistItem> WatchlistItems { get; }
    IRepository<SkillTrend> SkillTrends { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
