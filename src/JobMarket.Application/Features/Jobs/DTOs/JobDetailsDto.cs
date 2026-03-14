using JobMarket.Domain.Enums;

namespace JobMarket.Application.Features.Jobs.DTOs;

public class JobDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = new();
    public SeniorityLevel SeniorityLevel { get; set; }
    public JobType JobType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string Currency { get; set; } = string.Empty;
    public JobSource Source { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; }
}
