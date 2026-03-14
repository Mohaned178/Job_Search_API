namespace JobMarket.Application.Common.Interfaces;

public interface IJobEnqueuer
{
    void Enqueue<TJob>(System.Linq.Expressions.Expression<Func<TJob, Task>> methodCall);
}
