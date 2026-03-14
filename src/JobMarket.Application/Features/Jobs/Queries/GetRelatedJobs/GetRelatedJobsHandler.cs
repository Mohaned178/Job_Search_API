using AutoMapper;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.Jobs.DTOs;
using JobMarket.Domain.Exceptions;
using MediatR;

namespace JobMarket.Application.Features.Jobs.Queries.GetRelatedJobs;

public class GetRelatedJobsHandler : IRequestHandler<GetRelatedJobsQuery, List<JobSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVectorSearchService _vectorSearch;
    private readonly IMapper _mapper;

    public GetRelatedJobsHandler(
        IUnitOfWork unitOfWork,
        IVectorSearchService vectorSearch,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _vectorSearch = vectorSearch;
        _mapper = mapper;
    }

    public async Task<List<JobSummaryDto>> Handle(
        GetRelatedJobsQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Job), request.JobId);

        if (job.Embedding == null)
            return new List<JobSummaryDto>();

        var matches = await _vectorSearch.FindSimilarJobsAsync(
            job.Embedding,
            request.TopK + 1,
            null,
            cancellationToken);

        var relatedJobs = matches
            .Where(m => m.Job.Id != request.JobId)
            .Take(request.TopK)
            .Select(m => m.Job)
            .ToList();

        return _mapper.Map<List<JobSummaryDto>>(relatedJobs);
    }
}
