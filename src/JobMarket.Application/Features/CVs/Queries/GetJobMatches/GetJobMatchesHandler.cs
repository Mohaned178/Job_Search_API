using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.CVs.DTOs;
using JobMarket.Domain.Exceptions;
using MediatR;

namespace JobMarket.Application.Features.CVs.Queries.GetJobMatches;

public class GetJobMatchesHandler : IRequestHandler<GetJobMatchesQuery, List<JobMatchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVectorSearchService _vectorSearch;

    public GetJobMatchesHandler(IUnitOfWork unitOfWork, IVectorSearchService vectorSearch)
    {
        _unitOfWork = unitOfWork;
        _vectorSearch = vectorSearch;
    }

    public async Task<List<JobMatchDto>> Handle(GetJobMatchesQuery request, CancellationToken cancellationToken)
    {
        var cv = await _unitOfWork.CVs.GetByIdAsync(request.CvId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.CV), request.CvId);

        if (cv.UserId != request.UserId)
            throw new UnauthorizedException();

        if (cv.Embedding == null)
            throw new DomainException("CV embedding is not yet available. CV is still being processed.");

        var matches = await _vectorSearch.FindSimilarJobsAsync(
            cv.Embedding,
            request.TopK,
            null,
            cancellationToken);

        List<string> cvSkills = cv.ParsedData?.Skills ?? new List<string>();

        return matches.Select(m => new JobMatchDto
        {
            JobId = m.Job.Id,
            Title = m.Job.Title,
            Company = m.Job.Company,
            Location = $"{m.Job.City}, {m.Job.Country}",
            MatchScore = Math.Round(m.MatchScore * 100, 1),
            MatchedSkills = m.Job.RequiredSkills.Intersect(cvSkills, StringComparer.OrdinalIgnoreCase).ToList(),
            MissingSkills = m.Job.RequiredSkills.Except(cvSkills, StringComparer.OrdinalIgnoreCase).ToList()
        }).ToList();
    }
}
