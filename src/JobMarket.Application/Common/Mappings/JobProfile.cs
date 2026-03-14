using AutoMapper;
using JobMarket.Application.Features.Jobs.DTOs;
using JobMarket.Domain.Entities;

namespace JobMarket.Application.Common.Mappings;

public class JobProfile : Profile
{
    public JobProfile()
    {
        CreateMap<Job, JobSummaryDto>()
            .ForMember(d => d.Location, o => o.MapFrom(s => $"{s.City}, {s.Country}"))
            .ForMember(d => d.SalaryRange, o => o.MapFrom(s =>
                s.SalaryMin.HasValue
                    ? $"{s.SalaryMin} - {s.SalaryMax} {s.Currency}"
                    : "Not specified"));

        CreateMap<Job, JobDetailsDto>();
    }
}
