using AutoMapper;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.CVs.DTOs;
using JobMarket.Domain.Exceptions;
using MediatR;

namespace JobMarket.Application.Features.CVs.Queries.GetAtsScore;

public class GetAtsScoreHandler : IRequestHandler<GetAtsScoreQuery, AtsScoreResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAtsScoreHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AtsScoreResponse> Handle(GetAtsScoreQuery request, CancellationToken cancellationToken)
    {
        var cv = await _unitOfWork.CVs.GetByIdAsync(request.CvId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.CV), request.CvId);

        if (cv.UserId != request.UserId)
            throw new UnauthorizedException();

        if (cv.AtsScore == null || cv.AtsBreakdown == null)
            throw new DomainException("ATS score is not yet available. CV is still being processed.");

        return new AtsScoreResponse
        {
            CvId = cv.Id,
            Score = cv.AtsScore.Value,
            Breakdown = cv.AtsBreakdown
        };
    }
}
