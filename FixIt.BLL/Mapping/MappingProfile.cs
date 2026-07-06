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

        CreateMap<Issue, AdminIssueListItemDto>()
            .ForMember(dest => dest.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.FullName : "Unknown"));

        CreateMap<Issue, AdminIssueDetailsDto>()
            .ForMember(dest => dest.CitizenName, opt => opt.MapFrom(src => src.Citizen != null ? src.Citizen.FullName : "Unknown"))
            .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src.MaintenanceSchedule))
            .ForMember(dest => dest.Report, opt => opt.MapFrom(src => src.MaintenanceReport))
            .ForMember(dest => dest.Timeline, opt => opt.MapFrom(src => src.StatusHistory));

        CreateMap<IssueStatusHistory, TimelineEntryDto>()
            .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src => src.ChangedBy != null ? src.ChangedBy.FullName : "System"));

        // CreateIssueDto to Issue mapping
        CreateMap<FixIt.BLL.DTOs.CreateIssueDto, Issue>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.IssueId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.AdminNotes, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CitizenId, opt => opt.Ignore())
            .ForMember(dest => dest.AdminId, opt => opt.Ignore())
            .ForMember(dest => dest.Citizen, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore())
            .ForMember(dest => dest.MaintenanceSchedule, opt => opt.Ignore())
            .ForMember(dest => dest.MaintenanceReport, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.StatusHistory, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore());

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

        // Comment mappings
        CreateMap<IssueComment, IssueCommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Unknown"))
            .ForMember(dest => dest.UserProfileImageUrl, opt => opt.MapFrom(src => src.User != null ? src.User.ProfileImageUrl : null))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.User != null ? (src.User is Admin ? "Admin" : "Citizen") : "User"));
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
