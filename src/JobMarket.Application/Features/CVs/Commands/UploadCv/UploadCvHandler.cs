using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.CVs.DTOs;
using JobMarket.Domain.Entities;
using MediatR;

namespace JobMarket.Application.Features.CVs.Commands.UploadCv;

public class UploadCvHandler : IRequestHandler<UploadCvCommand, UploadCvResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;
    private readonly IJobEnqueuer _jobEnqueuer;

    public UploadCvHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService storage,
        IJobEnqueuer jobEnqueuer)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _jobEnqueuer = jobEnqueuer;
    }

    public async Task<UploadCvResponse> Handle(UploadCvCommand request, CancellationToken cancellationToken)
    {
        string storagePath = await _storage.UploadAsync(request.FileStream, request.FileName, cancellationToken);
        CV cv = CV.Create(request.UserId, request.FileName, storagePath);

        await _unitOfWork.CVs.AddAsync(cv, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _jobEnqueuer.Enqueue<ICvProcessingJob>(j => j.ProcessAsync(cv.Id, CancellationToken.None));

        return new UploadCvResponse(cv.Id, cv.EnrichmentStatus.ToString(), "CV uploaded and queued for processing.");
    }
}
