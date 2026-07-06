using FixIt.Common.Enums;

namespace FixIt.Common.DTOs;

public class AiClassificationResultDto
{
    public string SuggestedTitle { get; set; } = string.Empty;
    public string SuggestedDescription { get; set; } = string.Empty;
    public IssueCategory SuggestedCategory { get; set; }
    public IssuePriority SuggestedPriority { get; set; }
    public string CategoryReason { get; set; } = string.Empty;
    public string PriorityReason { get; set; } = string.Empty;
    public double Confidence { get; set; }
}
