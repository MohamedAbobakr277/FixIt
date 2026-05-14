namespace FixIt.Common.DTOs;

public class CitizenDashboardDto
{
    public string FullName { get; set; } = string.Empty;
    public int TotalIssues { get; set; }
    public int InProgressIssues { get; set; }
    public int ResolvedIssues { get; set; }
    public int NeedsRatingIssues { get; set; }
    public List<IssueListDto> RecentIssues { get; set; } = new();
}

public class CitizenProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsTwoFactorEnabled { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime MemberSince { get; set; }
    public string? ProfilePicture { get; set; }
    
    public int IssuesReported { get; set; }
    public int ResolvedIssues { get; set; }
    public int RatingsGiven { get; set; }
    
    public List<IssueListDto> IssueHistory { get; set; } = new();
    public List<RatingDetailsDto> RecentRatings { get; set; } = new();
}

public class RatingDetailsDto
{
    public int IssueId { get; set; }
    public string IssueTitle { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime RatedAt { get; set; }
}
