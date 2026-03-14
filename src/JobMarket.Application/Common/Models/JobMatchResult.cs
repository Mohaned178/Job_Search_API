using JobMarket.Domain.Entities;

namespace JobMarket.Application.Common.Models;

public class JobMatchResult
{
    public Job Job { get; set; } = null!;
    public double MatchScore { get; set; }
}
