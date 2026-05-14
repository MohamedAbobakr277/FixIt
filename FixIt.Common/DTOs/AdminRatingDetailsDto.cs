namespace FixIt.Common.DTOs;

public class AdminRatingDetailsDto
{
    public int RatingId { get; set; }
    public int IssueId { get; set; }
    public string IssueTitle { get; set; } = string.Empty;
    public string IssueStatus { get; set; } = string.Empty;
    public string CitizenName { get; set; } = string.Empty;
    public string CitizenEmail { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedAt { get; set; }
}
