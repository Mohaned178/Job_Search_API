using JobMarket.Domain.Enums;

namespace JobMarket.Application.Features.Jobs.DTOs;

public class JobSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string SalaryRange { get; set; } = string.Empty;
    public SeniorityLevel SeniorityLevel { get; set; }
    public JobType JobType { get; set; }
    public JobSource Source { get; set; }
    public DateTime PostedAt { get; set; }
}
