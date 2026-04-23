using AutoMapper;
using FixIt.DAL.Entities;

namespace FixIt.BLL.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Issue mappings (members will extend these)
        CreateMap<Issue, IssueSummaryDto>();
        CreateMap<Issue, FixIt.BLL.DTOs.IssueListDto>();
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

public class IssueDetailsDto
{
    public int IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public ScheduleDto? Schedule { get; set; }
    public ReportDto? Report { get; set; }
    public RatingDto? Rating { get; set; }
}

public class RatingDto
{
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class ScheduleDto
{
    public DateTime VisitDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string? WorkerName { get; set; }
}

public class ReportDto
{
    public string Summary { get; set; } = string.Empty;
    public string? WorkerNotes { get; set; }
    public string? BeforeImageUrl { get; set; }
    public string? AfterImageUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
}
