using AutoMapper;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.Jobs.DTOs;
using JobMarket.Domain.Entities;
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
        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, 100);

        var preferences = await _unitOfWork.UserPreferences
            .FindFirstAsync(p => p.UserId == request.UserId, cancellationToken);

        System.Linq.Expressions.Expression<Func<Job, bool>> predicate = preferences != null
            ? j =>
                (!preferences.PreferredJobType.HasValue || j.JobType == preferences.PreferredJobType) &&
                (!preferences.PreferredSeniority.HasValue || j.SeniorityLevel == preferences.PreferredSeniority) &&
                (preferences.PreferredLocations.Count == 0 || preferences.PreferredLocations.Contains(j.Country) || preferences.PreferredLocations.Contains(j.City))
            : j => true;

        (IReadOnlyList<Job> jobs, int totalCount) = await _unitOfWork.Jobs
            .FindPagedAsync(predicate, page, pageSize, cancellationToken);

        return new PaginatedResult<JobSummaryDto>
        {
            Items = _mapper.Map<List<JobSummaryDto>>(jobs),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
