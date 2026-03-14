using JobMarket.Application.Common.Interfaces;
using MediatR;

namespace JobMarket.Application.Features.Analytics.Queries.GetSalaryIntelligence;

public class GetSalaryIntelligenceHandler : IRequestHandler<GetSalaryIntelligenceQuery, SalaryIntelligenceDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSalaryIntelligenceHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SalaryIntelligenceDto> Handle(
        GetSalaryIntelligenceQuery request,
        CancellationToken cancellationToken)
    {
        var jobs = await _unitOfWork.Jobs.FindAsync(
            j => j.SalaryMin.HasValue &&
                 (string.IsNullOrEmpty(request.Role) || j.Title.Contains(request.Role)) &&
                 (string.IsNullOrEmpty(request.Location) || j.Country == request.Location || j.City == request.Location),
            cancellationToken);

        var salaries = jobs
            .Where(j => j.SalaryMin.HasValue && j.SalaryMax.HasValue)
            .Select(j => (j.SalaryMin!.Value + j.SalaryMax!.Value) / 2)
            .OrderBy(s => s)
            .ToList();

        if (salaries.Count == 0)
        {
            return new SalaryIntelligenceDto
            {
                Role = request.Role ?? "All",
                Location = request.Location ?? "Global",
                SampleSize = 0
            };
        }

        return new SalaryIntelligenceDto
        {
            Role = request.Role ?? "All",
            Location = request.Location ?? "Global",
            AverageSalary = Math.Round(salaries.Average(), 2),
            MedianSalary = salaries[salaries.Count / 2],
            MinSalary = salaries.Min(),
            MaxSalary = salaries.Max(),
            SampleSize = salaries.Count
        };
    }
}
