namespace JobMarket.Application.Common.Interfaces;

public interface ICvProcessingJob
{
    Task ProcessAsync(Guid cvId, CancellationToken ct);
}
