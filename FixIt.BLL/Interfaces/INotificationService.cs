using FixIt.DAL.Entities;

namespace FixIt.BLL.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, NotificationType type, string title, string message, string? url = null);
    Task PushNeighborhoodAnnouncementAsync(string locationQuery, string title, string message);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int count = 10);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int notificationId, string userId);
    
    // Admin Notifications
    Task CreateAdminNotificationAsync(AdminNotificationType type, string title, string message, string? relatedEntityId = null, string? relatedEntityUrl = null);
    Task<IEnumerable<AdminNotification>> GetAdminNotificationsAsync(int count = 20);
    Task<int> GetAdminUnreadCountAsync();
    Task MarkAdminNotificationAsReadAsync(int notificationId);
}
