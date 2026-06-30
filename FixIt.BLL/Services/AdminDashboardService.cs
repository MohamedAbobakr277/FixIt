using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace FixIt.BLL.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AdminDashboardService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var issues = _unitOfWork.Issues.GetAll();

        var stats = new DashboardStatsDto
        {
            TotalIssues      = await issues.CountAsync(),
            NewIssues        = await issues.CountAsync(i => i.Status == Common.Enums.IssueStatus.New),
            ApprovedIssues   = await issues.CountAsync(i => i.Status == Common.Enums.IssueStatus.Approved),
            ScheduledIssues  = await issues.CountAsync(i => i.Status == Common.Enums.IssueStatus.Scheduled),
            InProgressIssues = await issues.CountAsync(i => i.Status == Common.Enums.IssueStatus.InProgress),
            ResolvedIssues   = await issues.CountAsync(i => i.Status == Common.Enums.IssueStatus.Resolved),
            ClosedIssues     = await issues.CountAsync(i => i.Status == Common.Enums.IssueStatus.Closed),
            RejectedIssues   = await issues.CountAsync(i => i.Status == Common.Enums.IssueStatus.Rejected),
        };

        // Average rating
        var ratings = _unitOfWork.Ratings.GetAll();
        stats.TotalRatings = await ratings.CountAsync();
        stats.AverageRating = stats.TotalRatings > 0
            ? Math.Round(await ratings.AverageAsync(r => (double)r.Stars), 1)
            : 0.0;

        // Recent 5 issues (newest first) with citizen name
        stats.RecentIssues = await issues
            .OrderByDescending(i => i.SubmittedAt)
            .Take(5)
            .Include(i => i.Citizen)
            .Select(i => new RecentIssueDto
            {
                IssueId     = i.IssueId,
                Title       = i.Title,
                CitizenName = i.Citizen != null ? i.Citizen.FullName : "Unknown",
                Status      = i.Status
            })
            .ToListAsync();

        // 2. SLA Check
        var slaThreshold = DateTime.UtcNow.AddHours(-48);
        var overdueIssues = await issues
            .Where(i => i.Status == Common.Enums.IssueStatus.InProgress && i.UpdatedAt < slaThreshold)
            .ToListAsync();

        foreach (var overdue in overdueIssues)
        {
            // Check if notification already exists to avoid spamming
            var existingNotifs = await _unitOfWork.AdminNotifications.GetAll()
                .Where(n => n.RelatedEntityId == overdue.IssueId.ToString() && n.Type == DAL.Entities.AdminNotificationType.SlaWarning)
                .AnyAsync();
                
            if (!existingNotifs)
            {
                await _notificationService.CreateAdminNotificationAsync(
                    DAL.Entities.AdminNotificationType.SlaWarning,
                    "⏱️ SLA Warning",
                    $"Issue '{overdue.Title}' has been In Progress for over 48 hours.",
                    overdue.IssueId.ToString(),
                    $"/Issue/AdminDetails?id={overdue.IssueId}"
                );
            }
        }

        return stats;
    }
}
