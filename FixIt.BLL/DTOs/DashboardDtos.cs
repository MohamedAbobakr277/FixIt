using FixIt.Common.Enums;

namespace FixIt.BLL.DTOs;

public class DashboardStatsDto
{
    public int TotalIssues { get; set; }
    public int NewIssues { get; set; }
    public int InProgressIssues { get; set; }
    public int ResolvedIssues { get; set; }
    public int ApprovedIssues { get; set; }
    public int ScheduledIssues { get; set; }
    public int RejectedIssues { get; set; }
    public int ClosedIssues { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public List<RecentIssueDto> RecentIssues { get; set; } = new();
}

public class RecentIssueDto
{
    public int IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CitizenName { get; set; } = string.Empty;
    public IssueStatus Status { get; set; }
}
