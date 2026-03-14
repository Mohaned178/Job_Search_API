using JobMarket.Domain.Enums;

namespace JobMarket.Application.Common.Models;

public class JobEnrichmentResult
{
    public List<string> Skills { get; set; } = new();
    public SeniorityLevel SeniorityLevel { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string Currency { get; set; } = string.Empty;
}
