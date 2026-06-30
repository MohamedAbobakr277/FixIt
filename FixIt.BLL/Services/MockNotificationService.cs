using FixIt.BLL.Interfaces;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace FixIt.BLL.Services;

public class MockNotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MockNotificationService> _logger;

    public MockNotificationService(IUnitOfWork unitOfWork, ILogger<MockNotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendNotificationAsync(string userId, NotificationType type, string title, string message, string? url = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityUrl = url,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.CompleteAsync();

        // Simulate sending via 3rd party (Twilio, WhatsApp API, APNS/FCM)
        _logger.LogInformation($"[MOCK {type.ToString().ToUpper()}] To User {userId}: {title} - {message}");
    }

    public async Task PushNeighborhoodAnnouncementAsync(string locationQuery, string title, string message)
    {
        // 1. Find all issues that match the location query loosely, then get those citizens.
        // OR a better approach: find all citizens where Address/Location matches the query.
        // Wait, Citizen doesn't have an explicit Location property, it's tied to Issues or their ApplicationUser Address.
        
        // Let's assume we find all users with address containing locationQuery.
        // But since Address might not be widely populated, let's just get distinct Citizens who have submitted issues in that location.
        var userIds = await _unitOfWork.Issues.GetAll()
            .Where(i => i.Location.Contains(locationQuery))
            .Select(i => i.CitizenId)
            .Distinct()
            .ToListAsync();

        if (!userIds.Any())
        {
            _logger.LogWarning($"[MOCK NEIGHBORHOOD ANNOUNCEMENT] No users found in area: {locationQuery}");
            return;
        }

        foreach (var userId in userIds)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.SMS, // Defaulting to SMS for urgent announcements
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Notifications.AddAsync(notification);
        }
        
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation($"[MOCK NEIGHBORHOOD ANNOUNCEMENT] Sent SMS to {userIds.Count} users in area '{locationQuery}': {title} - {message}");
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int count = 10)
    {
        return await _unitOfWork.Notifications.GetAll()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _unitOfWork.Notifications.GetAll()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        if (notification != null && notification.UserId == userId && !notification.IsRead)
        {
            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.CompleteAsync();
        }
    }

    public async Task CreateAdminNotificationAsync(AdminNotificationType type, string title, string message, string? relatedEntityId = null, string? relatedEntityUrl = null)
    {
        var adminNotif = new AdminNotification
        {
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityId = relatedEntityId,
            RelatedEntityUrl = relatedEntityUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.AdminNotifications.AddAsync(adminNotif);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<IEnumerable<AdminNotification>> GetAdminNotificationsAsync(int count = 20)
    {
        return await _unitOfWork.AdminNotifications.GetAll()
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetAdminUnreadCountAsync()
    {
        return await _unitOfWork.AdminNotifications.GetAll()
            .CountAsync(n => !n.IsRead);
    }

    public async Task MarkAdminNotificationAsReadAsync(int notificationId)
    {
        var notification = await _unitOfWork.AdminNotifications.GetByIdAsync(notificationId);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            _unitOfWork.AdminNotifications.Update(notification);
            await _unitOfWork.CompleteAsync();
        }
    }
}
