using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Exceptions;
using MediatR;

namespace JobMarket.Application.Features.CVs.Commands.DeleteCv;

public class DeleteCvHandler : IRequestHandler<DeleteCvCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;

    public DeleteCvHandler(IUnitOfWork unitOfWork, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task<Unit> Handle(DeleteCvCommand request, CancellationToken cancellationToken)
    {
        var cv = await _unitOfWork.CVs.GetByIdAsync(request.CvId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.CV), request.CvId);

        if (cv.UserId != request.UserId)
            throw new UnauthorizedException();

        await _storage.DeleteAsync(cv.StoragePath, cancellationToken);
        await _unitOfWork.CVs.DeleteAsync(cv, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
