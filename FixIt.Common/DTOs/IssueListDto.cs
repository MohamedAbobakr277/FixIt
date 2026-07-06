using System;
using FixIt.Common.Enums;

namespace FixIt.Common.DTOs;

public class IssueListDto
{
    public int IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public IssueCategory Category { get; set; }
    public IssueStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
}
