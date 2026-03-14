using JobMarket.Domain.Enums;

namespace JobMarket.Application.Features.CVs.DTOs;

public class CvStatusResponse
{
    public Guid CvId { get; set; }
    public EnrichmentStatus Status { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
