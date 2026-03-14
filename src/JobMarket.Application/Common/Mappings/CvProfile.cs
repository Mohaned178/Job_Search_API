using AutoMapper;
using JobMarket.Application.Features.CVs.DTOs;
using JobMarket.Domain.Entities;
using JobMarket.Domain.Enums;

namespace JobMarket.Application.Common.Mappings;

public class CvProfile : Profile
{
    public CvProfile()
    {
        CreateMap<CV, UploadCvResponse>()
            .ForMember(d => d.CvId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.EnrichmentStatus.ToString()))
            .ForMember(d => d.Message, o => o.Ignore());

        CreateMap<CV, CvStatusResponse>()
            .ForMember(d => d.CvId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UploadedAt, o => o.MapFrom(s => s.CreatedAt));
    }
}
