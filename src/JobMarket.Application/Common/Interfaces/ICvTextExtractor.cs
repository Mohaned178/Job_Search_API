namespace JobMarket.Application.Common.Interfaces;

public interface ICvTextExtractor
{
    Task<string> ExtractAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
