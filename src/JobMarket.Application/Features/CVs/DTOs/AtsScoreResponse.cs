using JobMarket.Domain.ValueObjects;

namespace JobMarket.Application.Features.CVs.DTOs;

public class AtsScoreResponse
{
    public Guid CvId { get; set; }
    public int Score { get; set; }
    public AtsScoreBreakdown Breakdown { get; set; } = null!;
}
