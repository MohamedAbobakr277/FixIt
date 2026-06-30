using System;

namespace FixIt.Common.DTOs;

public class IssueCommentDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserProfileImageUrl { get; set; }
    public string UserType { get; set; } = string.Empty; // "Citizen" or "Admin"
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
