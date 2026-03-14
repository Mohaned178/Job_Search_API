using System.Linq.Expressions;
using JobMarket.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Jobs;

public class HangfireJobEnqueuer : IJobEnqueuer
{
    private readonly ILogger<HangfireJobEnqueuer> _logger;

    public HangfireJobEnqueuer(ILogger<HangfireJobEnqueuer> logger)
    {
        _logger = logger;
    }

    public void Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall)
    {
        _logger.LogInformation("Enqueueing background job of type {JobType}", typeof(TJob).Name);
        Hangfire.BackgroundJob.Enqueue(methodCall);
    }
}
