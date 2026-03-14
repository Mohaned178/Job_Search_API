using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Entities;
using JobMarket.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JobMarket.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public IRepository<Job> Jobs { get; }
    public IRepository<CV> CVs { get; }
    public IRepository<User> Users { get; }
    public IRepository<UserPreferences> UserPreferences { get; }
    public IRepository<JobAlert> JobAlerts { get; }
    public IRepository<WatchlistItem> WatchlistItems { get; }
    public IRepository<SkillTrend> SkillTrends { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Jobs = new GenericRepository<Job>(context);
        CVs = new GenericRepository<CV>(context);
        Users = new GenericRepository<User>(context);
        UserPreferences = new GenericRepository<UserPreferences>(context);
        JobAlerts = new GenericRepository<JobAlert>(context);
        WatchlistItems = new GenericRepository<WatchlistItem>(context);
        SkillTrends = new GenericRepository<SkillTrend>(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
        await _transaction!.CommitAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
        => await _transaction!.RollbackAsync(ct);

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
