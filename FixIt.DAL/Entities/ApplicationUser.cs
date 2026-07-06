using Microsoft.AspNetCore.Identity;

namespace FixIt.DAL.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 2FA Fields
    public string? TwoFactorSecret { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public string? RecoveryCodes { get; set; } // Semi-colon separated or JSON

    // Profile Picture
    public string? ProfileImageUrl { get; set; }

    // Notification Preferences
    public bool EmailIssueUpdates { get; set; } = true;
    public bool EmailMaintenanceAlerts { get; set; } = true;
    public bool EmailWeeklyReports { get; set; } = true;
    public bool AppRealTimePush { get; set; } = true;
    public bool AppDirectMessages { get; set; } = true;
}
