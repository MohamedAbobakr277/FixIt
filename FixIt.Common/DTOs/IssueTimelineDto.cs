namespace FixIt.Common.DTOs;

public class IssueTimelineDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
