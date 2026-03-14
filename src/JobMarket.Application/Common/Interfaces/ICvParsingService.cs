using JobMarket.Domain.ValueObjects;

namespace JobMarket.Application.Common.Interfaces;

public interface ICvParsingService
{
    Task<CvParsedData> ParseAsync(string rawText, CancellationToken ct = default);
}
