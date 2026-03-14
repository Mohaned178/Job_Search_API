using JobMarket.Domain.Enums;

namespace JobMarket.Application.Features.Jobs.DTOs;

public class JobSearchFilter
{
    public string? Keyword { get; set; }
    public string? Category { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public SeniorityLevel? SeniorityLevel { get; set; }
    public JobType? JobType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
