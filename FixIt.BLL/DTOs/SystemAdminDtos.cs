namespace FixIt.BLL.DTOs;

public class SystemDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalCitizens { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalSystemAdmins { get; set; }
    public int TotalIssues { get; set; }
    public int ActiveIssues { get; set; }
    public int ResolvedIssues { get; set; }
}

public class UserManagementDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsLockedOut { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SystemLogDto
{
    public string UserEmail { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public DateTime LoginTime { get; set; }
    public string Device { get; set; } = string.Empty;
}
