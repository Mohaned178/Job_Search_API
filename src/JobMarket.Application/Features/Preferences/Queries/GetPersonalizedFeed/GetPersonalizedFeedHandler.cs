using AutoMapper;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.Jobs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Preferences.Queries.GetPersonalizedFeed;

public class GetPersonalizedFeedHandler : IRequestHandler<GetPersonalizedFeedQuery, PaginatedResult<JobSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPersonalizedFeedHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<JobSummaryDto>> Handle(
        GetPersonalizedFeedQuery request,
        CancellationToken cancellationToken)
    {
        var userPrefs = await _unitOfWork.UserPreferences
            .FindAsync(p => p.UserId == request.UserId, cancellationToken);

        var preferences = userPrefs.FirstOrDefault();

        var jobs = preferences != null
            ? await _unitOfWork.Jobs.FindAsync(j =>
                (!preferences.PreferredJobType.HasValue || j.JobType == preferences.PreferredJobType) &&
                (!preferences.PreferredSeniority.HasValue || j.SeniorityLevel == preferences.PreferredSeniority) &&
                (preferences.PreferredLocations.Count == 0 || preferences.PreferredLocations.Contains(j.Country) || preferences.PreferredLocations.Contains(j.City)),
                cancellationToken)
            : await _unitOfWork.Jobs.GetAllAsync(cancellationToken);

        int totalCount = jobs.Count;
        var paginatedJobs = jobs
            .OrderByDescending(j => j.PostedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResult<JobSummaryDto>
        {
            Items = _mapper.Map<List<JobSummaryDto>>(paginatedJobs),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
