using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace FixIt.BLL.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminDashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

        return stats;
    }
}
