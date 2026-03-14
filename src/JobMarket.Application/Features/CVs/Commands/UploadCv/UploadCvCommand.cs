using JobMarket.Application.Features.CVs.DTOs;
using MediatR;

namespace JobMarket.Application.Features.CVs.Commands.UploadCv;

public record UploadCvCommand(Guid UserId, Stream FileStream, string FileName) : IRequest<UploadCvResponse>;
