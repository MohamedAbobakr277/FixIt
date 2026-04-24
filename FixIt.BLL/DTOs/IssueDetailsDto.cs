using System;

namespace FixIt.BLL.DTOs;

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
