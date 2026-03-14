using MediatR;

namespace JobMarket.Application.Features.CVs.Commands.DeleteCv;

public record DeleteCvCommand(Guid CvId, Guid UserId) : IRequest<Unit>;
