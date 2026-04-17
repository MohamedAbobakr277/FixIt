using FixIt.Common.Enums;

namespace FixIt.DAL.Entities;

public class Analytics
{
    public int TotalIssues { get; set; }
    public int ResolvedIssues { get; set; }
    public int PendingIssues { get; set; }
    public int RejectedIssues { get; set; }
    public float AverageRating { get; set; }
    public Dictionary<string, int> IssuesByCategory { get; set; }
    public Dictionary<string, int> IssuesByStatus { get; set; }
}