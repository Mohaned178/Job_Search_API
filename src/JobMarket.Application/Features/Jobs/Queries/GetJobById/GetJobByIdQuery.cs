using JobMarket.Application.Features.Jobs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Jobs.Queries.GetJobById;

public record GetJobByIdQuery(Guid Id) : IRequest<JobDetailsDto>;
