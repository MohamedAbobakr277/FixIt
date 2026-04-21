namespace FixIt.DAL.Entities;

public class MaintenanceReport
{
    public int ReportId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? WorkerNotes { get; set; }
    public string? BeforeImageUrl { get; set; }
    public string? AfterImageUrl { get; set; }
    public DateTime SubmittedAt { get; set; }

    // Foreign Keys
    public int IssueId { get; set; }
    public string AdminId { get; set; } = string.Empty;

    // Navigation properties
    public Issue? Issue { get; set; }
    public Admin? Admin { get; set; }
}