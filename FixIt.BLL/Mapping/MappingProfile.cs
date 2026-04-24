using AutoMapper;
using FixIt.Common.DTOs;
using FixIt.DAL.Entities;

namespace FixIt.BLL.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Issue mappings
        CreateMap<Issue, IssueSummaryDto>();
        CreateMap<Issue, IssueDetailsDto>()
            .ForMember(dest => dest.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.FullName : ""));

        // Rating mappings
        CreateMap<Rating, RatingDto>();

        // Schedule mappings
        CreateMap<MaintenanceSchedule, ScheduleDto>();

        // Report mappings
        CreateMap<MaintenanceReport, ReportDto>();
    }
}
