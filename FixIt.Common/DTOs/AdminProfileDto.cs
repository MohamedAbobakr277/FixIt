namespace FixIt.Common.DTOs;

public class AdminProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsTwoFactorEnabled { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public DateTime MemberSince { get; set; }
    public string? ProfilePicture { get; set; }

    // Admin-specific stats
    public int TotalIssuesManaged { get; set; }
    public int ResolvedIssues { get; set; }
    public int PendingIssues { get; set; }

    // Notification Preferences
    public bool EmailIssueUpdates { get; set; }
    public bool EmailMaintenanceAlerts { get; set; }
    public bool EmailWeeklyReports { get; set; }
    public bool AppRealTimePush { get; set; }
    public bool AppDirectMessages { get; set; }
}
