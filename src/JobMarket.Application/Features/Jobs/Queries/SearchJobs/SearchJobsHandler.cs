using System.Linq.Expressions;
using AutoMapper;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.Jobs.DTOs;
using JobMarket.Domain.Entities;
using MediatR;

namespace JobMarket.Application.Features.Jobs.Queries.SearchJobs;

public class SearchJobsHandler : IRequestHandler<SearchJobsQuery, PaginatedResult<JobSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchJobsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<JobSummaryDto>> Handle(
        SearchJobsQuery request,
        CancellationToken cancellationToken)
    {
        JobSearchFilter filter = request.Filter;
        int page = Math.Max(1, filter.Page);
        int pageSize = Math.Clamp(filter.PageSize, 1, 100);

        Expression<Func<Job, bool>> predicate = j =>
            (string.IsNullOrEmpty(filter.Keyword) || j.Title.Contains(filter.Keyword) || j.Description.Contains(filter.Keyword)) &&
            (string.IsNullOrEmpty(filter.Country) || j.Country == filter.Country) &&
            (string.IsNullOrEmpty(filter.City) || j.City == filter.City) &&
            (!filter.SeniorityLevel.HasValue || j.SeniorityLevel == filter.SeniorityLevel) &&
            (!filter.JobType.HasValue || j.JobType == filter.JobType) &&
            (!filter.SalaryMin.HasValue || j.SalaryMin >= filter.SalaryMin) &&
            (!filter.SalaryMax.HasValue || j.SalaryMax <= filter.SalaryMax);

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
