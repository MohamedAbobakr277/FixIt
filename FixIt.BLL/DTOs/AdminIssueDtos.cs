using FixIt.Common.DTOs;
using FixIt.Common.Enums;

namespace FixIt.BLL.DTOs;

public class AdminIssueListItemDto
{
    public int IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CitizenName { get; set; } = string.Empty;
    public IssueCategory Category { get; set; }
    public IssueStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class AdminIssueListPageDto
{
    public List<AdminIssueListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public string? SearchTerm { get; set; }
    public IssueStatus? StatusFilter { get; set; }
}

public class AdminIssueDetailsDto
{
    public int IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ImageUrl { get; set; }
    public IssueCategory Category { get; set; }
    public IssueStatus Status { get; set; }
    public IssuePriority Priority { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public ScheduleDto? Schedule { get; set; }
    public ReportDto? Report { get; set; }
    public bool IsPaid { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? PaidCurrency { get; set; }
    public DateTime? PaymentCompletedAt { get; set; }
    public string? PaymentTransactionId { get; set; }
    public List<TimelineEntryDto> Timeline { get; set; } = new();
    public IEnumerable<IssueCommentDto> Comments { get; set; } = new List<IssueCommentDto>();
}

public class TimelineEntryDto
{
    public IssueStatus Status { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Note { get; set; }
    public string? ChangedByName { get; set; }
}

public class SaveAdminNotesDto
{
    public int IssueId { get; set; }
    public string? AdminNotes { get; set; }
}

