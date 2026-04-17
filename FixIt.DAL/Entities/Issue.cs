using FixIt.Common.Enums;

namespace FixIt.DAL.Entities;
    public class Issue
{
    public int IssueId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string? ImageUrl { get; set; }
    public IssueStatus Status { get; set; } = IssueStatus.New;
    public IssueCategory Category { get; set; }
    public IssuePriority Priority { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
 
    // Foreign keys
    public int CitizenId { get; set; }
    public int? AdminId { get; set; }
 
    // Navigation properties
    public Citizen? Citizen { get; set; }
    public Admin? Admin { get; set; }
    public MaintenanceSchedule? MaintenanceSchedule { get; set; }
    public MaintenanceReport? MaintenanceReport { get; set; }
    public Rating? Rating { get; set; }
}
