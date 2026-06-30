using System;

namespace FixIt.DAL.Entities;

public enum AdminNotificationType
{
    HighPriorityIssue,
    SlaWarning,
    CitizenCommunication,
    SystemAlert
}

public class AdminNotification
{
    public int Id { get; set; }
    public AdminNotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityId { get; set; }
    public string? RelatedEntityUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
