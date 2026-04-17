namespace FixIt.DAL.Entities;

public class MaintenanceReport
{
    public int ReportId { get; set; }
    public string Summary { get; set; }
    public string? WorkerNotes { get; set; }
    public string? BeforeImageUrl { get; set; }
    public string? AfterImageUrl { get; set; }
    public DateTime SubmittedAt { get; set; }

    // Foreign Keys
    public int IssueId { get; set; }
    public int AdminId { get; set; }

    // Navigation properties
    public Issue? Issue { get; set; }
    public Admin? Admin { get; set; }
}