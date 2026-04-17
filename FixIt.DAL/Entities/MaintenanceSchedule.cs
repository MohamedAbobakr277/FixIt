namespace FixIt.DAL.Entities;

public class MaintenanceSchedule
{
    public int ScheduleId { get; set; }
    public DateTime VisitDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string? WorkerName { get; set; }
    public DateTime CreatedAt { get; set; }

    // Foreign Key
    public int IssueId { get; set; }

    // Navigation property
    public Issue? Issue { get; set; }
}