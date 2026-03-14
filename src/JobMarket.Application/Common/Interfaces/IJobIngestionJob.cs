namespace JobMarket.Application.Common.Interfaces;

public interface IJobIngestionJob
{
    Task IngestAsync(CancellationToken ct);
}
