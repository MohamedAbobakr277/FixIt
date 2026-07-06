using FixIt.Common.Enums;

namespace FixIt.Common.DTOs;

public class CreateIssueDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public IssueCategory Category { get; set; }
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;
    // IFormFile moved to BLL layer to keep Common framework-independent
}
