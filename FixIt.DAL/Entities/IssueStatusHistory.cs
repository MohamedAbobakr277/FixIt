using FixIt.Common.Enums;

namespace FixIt.DAL.Entities;

public class IssueStatusHistory
{
    public int IssueStatusHistoryId { get; set; }
    public int IssueId { get; set; }
    public IssueStatus Status { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedById { get; set; }
    public string? Note { get; set; }

    public Issue? Issue { get; set; }
    public ApplicationUser? ChangedBy { get; set; }
}
