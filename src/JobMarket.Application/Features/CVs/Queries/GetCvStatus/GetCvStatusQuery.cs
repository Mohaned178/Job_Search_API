using JobMarket.Application.Features.CVs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.CVs.Queries.GetCvStatus;

public record GetCvStatusQuery(Guid CvId, Guid UserId) : IRequest<CvStatusResponse>;
