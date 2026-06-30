using System;

namespace FixIt.DAL.Entities;

public class IssueComment
{
    public int Id { get; set; }
    
    public int IssueId { get; set; }
    public Issue? Issue { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
