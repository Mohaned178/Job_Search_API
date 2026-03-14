using JobMarket.Domain.ValueObjects;

namespace JobMarket.Application.Common.Interfaces;

public interface IAtsService
{
    Task<(int Score, AtsScoreBreakdown Breakdown)> ScoreAsync(CvParsedData cvData, CancellationToken ct = default);
}
