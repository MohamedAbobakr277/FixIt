using FixIt.Common.Enums;

namespace FixIt.DAL.Entities;
    public class Admin : User
{
    public string? Department { get; set; }

    // Navigation properties
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    public ICollection<MaintenanceReport> MaintenanceReports { get; set; } = new List<MaintenanceReport>();
 
    public Admin()
    {
        Role = UserRole.Admin;
    }
}