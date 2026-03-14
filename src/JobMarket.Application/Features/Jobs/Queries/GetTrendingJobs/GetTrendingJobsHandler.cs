using AutoMapper;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.Jobs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Jobs.Queries.GetTrendingJobs;

public class GetTrendingJobsHandler : IRequestHandler<GetTrendingJobsQuery, List<JobSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTrendingJobsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<JobSummaryDto>> Handle(
        GetTrendingJobsQuery request,
        CancellationToken cancellationToken)
    {
        DateTime cutoff = request.TimeWindow switch
        {
            "1h" => DateTime.UtcNow.AddHours(-1),
            "6h" => DateTime.UtcNow.AddHours(-6),
            "7d" => DateTime.UtcNow.AddDays(-7),
            _ => DateTime.UtcNow.AddHours(-24)
        };

        var recentJobs = await _unitOfWork.Jobs
            .FindAsync(j => j.PostedAt >= cutoff, cancellationToken);

        var trendingJobs = recentJobs
            .OrderByDescending(j => j.PostedAt)
            .Take(request.Count)
            .ToList();

        return _mapper.Map<List<JobSummaryDto>>(trendingJobs);
    }
}
