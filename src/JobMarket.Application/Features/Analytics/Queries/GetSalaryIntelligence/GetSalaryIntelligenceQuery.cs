using MediatR;

namespace JobMarket.Application.Features.Analytics.Queries.GetSalaryIntelligence;

public class SalaryIntelligenceDto
{
    public string Role { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal AverageSalary { get; set; }
    public decimal MedianSalary { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public int SampleSize { get; set; }
}

public record GetSalaryIntelligenceQuery(
    string? Role = null,
    string? Location = null) : IRequest<SalaryIntelligenceDto>;
