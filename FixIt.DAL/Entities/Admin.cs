namespace FixIt.DAL.Entities;

public class Admin : ApplicationUser
{
    public string? Department { get; set; }

    // Navigation properties
    public ICollection<Issue> AssignedIssues { get; set; } = new List<Issue>();
    public ICollection<MaintenanceReport> MaintenanceReports { get; set; } = new List<MaintenanceReport>();
}