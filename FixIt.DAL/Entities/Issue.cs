using FixIt.Common.Enums;

namespace FixIt.DAL.Entities;

public class Issue
{
    public int IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ImageUrl { get; set; }
    public IssueStatus Status { get; set; } = IssueStatus.New;
    public IssueCategory Category { get; set; }
    public IssuePriority Priority { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Foreign keys (string IDs for Identity)
    public string CitizenId { get; set; } = string.Empty;
    public string? AdminId { get; set; }

    // Navigation properties
    public Citizen? Citizen { get; set; }
    public Admin? Admin { get; set; }
    public MaintenanceSchedule? MaintenanceSchedule { get; set; }
    public MaintenanceReport? MaintenanceReport { get; set; }
    public Rating? Rating { get; set; }
    public ICollection<IssueStatusHistory> StatusHistory { get; set; } = new List<IssueStatusHistory>();
    public ICollection<IssueComment> Comments { get; set; } = new List<IssueComment>();
}
