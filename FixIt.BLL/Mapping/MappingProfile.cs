using AutoMapper;
using FixIt.BLL.DTOs;
using FixIt.Common.DTOs;
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
            .ForMember(dest => dest.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.FullName : ""))
            .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src.MaintenanceSchedule))
            .ForMember(dest => dest.Report, opt => opt.MapFrom(src => src.MaintenanceReport));

        // CreateIssueDto to Issue mapping
        CreateMap<FixIt.BLL.DTOs.CreateIssueDto, Issue>()
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
        CreateMap<Rating, AdminRatingListDto>()
            .ForMember(dest => dest.IssueTitle, opt => opt.MapFrom(src => src.Issue != null ? src.Issue.Title : ""))
            .ForMember(dest => dest.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.FullName : ""));
        CreateMap<Rating, AdminRatingDetailsDto>()
            .ForMember(dest => dest.IssueTitle, opt => opt.MapFrom(src => src.Issue != null ? src.Issue.Title : ""))
            .ForMember(dest => dest.IssueStatus, opt => opt.MapFrom(src => src.Issue != null ? src.Issue.Status.ToString() : ""))
            .ForMember(dest => dest.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.FullName : ""))
            .ForMember(dest => dest.CitizenEmail, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.Email : ""));

        // Schedule mappings
        CreateMap<MaintenanceSchedule, ScheduleDto>();

        // Report mappings
        CreateMap<MaintenanceReport, ReportDto>();
    }
}

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
