using AutoMapper;
<<<<<<< HEAD
using FixIt.BLL.DTOs;
=======
using FixIt.Common.DTOs;
>>>>>>> feature/issue-details
using FixIt.DAL.Entities;

namespace FixIt.BLL.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Issue mappings
        CreateMap<Issue, IssueSummaryDto>();
        CreateMap<Issue, IssueListDto>();
        CreateMap<Issue, IssueDetailsDto>()
            .ForMember(dest => dest.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.FullName : ""));

        // CreateIssueDto to Issue mapping
        CreateMap<CreateIssueDto, Issue>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.IssueId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Priority, opt => opt.Ignore())
            .ForMember(dest => dest.AdminNotes, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CitizenId, opt => opt.Ignore())
            .ForMember(dest => dest.AdminId, opt => opt.Ignore())
            .ForMember(dest => dest.Citizen, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore())
            .ForMember(dest => dest.MaintenanceSchedule, opt => opt.Ignore())
            .ForMember(dest => dest.MaintenanceReport, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore());

        // Rating mappings
        CreateMap<Rating, RatingDto>();

        // Schedule mappings
        CreateMap<MaintenanceSchedule, ScheduleDto>();

        // Report mappings
        CreateMap<MaintenanceReport, ReportDto>();
    }
}
<<<<<<< HEAD

// ── Shared DTOs (members will add more in their own files) ──

public class IssueSummaryDto
{
    public int IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
=======
>>>>>>> feature/issue-details
