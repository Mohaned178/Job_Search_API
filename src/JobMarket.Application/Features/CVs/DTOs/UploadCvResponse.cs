namespace JobMarket.Application.Features.CVs.DTOs;

public record UploadCvResponse(Guid CvId, string Status, string Message);
