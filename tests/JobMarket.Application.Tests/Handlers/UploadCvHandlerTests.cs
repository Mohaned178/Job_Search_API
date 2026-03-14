using FluentAssertions;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.CVs.Commands.UploadCv;
using JobMarket.Application.Features.CVs.DTOs;
using JobMarket.Domain.Entities;
using Moq;

namespace JobMarket.Application.Tests.Handlers;

public class UploadCvHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IFileStorageService> _storageMock = new();
    private readonly Mock<IJobEnqueuer> _enqueueMock = new();
    private readonly Mock<IRepository<CV>> _cvRepoMock = new();

    private UploadCvHandler CreateHandler()
    {
        _unitOfWorkMock.Setup(u => u.CVs).Returns(_cvRepoMock.Object);
        return new UploadCvHandler(
            _unitOfWorkMock.Object,
            _storageMock.Object,
            _enqueueMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidFile_ReturnsCvIdAndStatus()
    {
        Guid userId = Guid.NewGuid();
        var stream = new System.IO.MemoryStream(new byte[] { 1, 2, 3 });

        _storageMock.Setup(s => s.UploadAsync(stream, "resume.pdf", default))
            .ReturnsAsync("/storage/resume.pdf");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = CreateHandler();
        var result = await handler.Handle(new UploadCvCommand(userId, stream, "resume.pdf"), default);

        result.Should().BeOfType<UploadCvResponse>();
        result.CvId.Should().NotBeEmpty();
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_ValidUpload_EnqueuesProcessingJob()
    {
        Guid userId = Guid.NewGuid();
        var stream = new System.IO.MemoryStream();

        _storageMock.Setup(s => s.UploadAsync(stream, "cv.pdf", default))
            .ReturnsAsync("/storage/cv.pdf");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = CreateHandler();
        await handler.Handle(new UploadCvCommand(userId, stream, "cv.pdf"), default);

        _enqueueMock.Verify(e => e.Enqueue<ICvProcessingJob>(
            It.IsAny<System.Linq.Expressions.Expression<Func<ICvProcessingJob, Task>>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpload_SavesChanges()
    {
        Guid userId = Guid.NewGuid();
        var stream = new System.IO.MemoryStream();

        _storageMock.Setup(s => s.UploadAsync(stream, "cv.pdf", default)).ReturnsAsync("/path");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = CreateHandler();
        await handler.Handle(new UploadCvCommand(userId, stream, "cv.pdf"), default);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
