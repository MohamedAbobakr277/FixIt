using FixIt.BLL.Interfaces;
using FixIt.BLL.DTOs;
using FixIt.DAL.Entities;
using FixIt.DAL.Repositories;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using FixIt.Common.Enums;

namespace FixIt.BLL.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SchedulePageDto> GetSchedulePageAsync()
    {
        var schedules = await _unitOfWork.Schedules
            .GetAll(s => s.Issue)
            .ToListAsync();

        var inProgressItems = schedules
            .Where(s => s.Issue?.Status == IssueStatus.InProgress)
            .Select(s => new ScheduleItemDto
            {
                IssueId = s.IssueId,
                ScheduleId = s.ScheduleId,
                IssueTitle = s.Issue?.Title ?? string.Empty,
                Location = s.Issue?.Location ?? string.Empty,
                Category = s.Issue?.Category.ToString() ?? string.Empty,
                Status = s.Issue?.Status.ToString() ?? string.Empty,
                VisitDate = s.VisitDate,
                WorkerName = s.WorkerName,
                EstimatedCost = s.EstimatedCost
            })
            .ToList();

        var scheduledItems = schedules
            .Where(s => s.Issue?.Status == IssueStatus.Scheduled)
            .OrderBy(s => s.VisitDate)
            .Select(s => new ScheduleItemDto
            {
                IssueId = s.IssueId,
                ScheduleId = s.ScheduleId,
                IssueTitle = s.Issue?.Title ?? string.Empty,
                Location = s.Issue?.Location ?? string.Empty,
                Category = s.Issue?.Category.ToString() ?? string.Empty,
                Status = s.Issue?.Status.ToString() ?? string.Empty,
                VisitDate = s.VisitDate,
                WorkerName = s.WorkerName,
                EstimatedCost = s.EstimatedCost
            })
            .ToList();

        return new SchedulePageDto
        {
            ScheduledCount = scheduledItems.Count,
            InProgressCount = inProgressItems.Count,
            InProgressItems = inProgressItems,
            UpcomingItems = scheduledItems
        };
    }

    public async Task<CreateScheduleDto?> GetCreateScheduleDtoAsync(int issueId)
    {
        var issue = await _unitOfWork.Issues
            .GetByIdAsync(issueId);

        if (issue == null || issue.Status != IssueStatus.Approved)
            return null;

        return new CreateScheduleDto
        {
            IssueId = issueId,
            VisitDate = DateTime.Now.AddDays(1), // Default to tomorrow
            EstimatedCost = 0
        };
    }

    public async Task<bool> CreateScheduleAsync(CreateScheduleDto dto)
    {
        var issue = await _unitOfWork.Issues
            .GetByIdAsync(dto.IssueId);

        if (issue == null || issue.Status != IssueStatus.Approved)
            return false;

        var schedule = new MaintenanceSchedule
        {
            IssueId = dto.IssueId,
            VisitDate = dto.VisitDate,
            WorkerName = dto.WorkerName,
            EstimatedCost = dto.EstimatedCost,
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Schedules.AddAsync(schedule);
        
        // Update issue status to Scheduled
        issue.Status = IssueStatus.Scheduled;
        issue.UpdatedAt = DateTime.Now;
        _unitOfWork.Issues.Update(issue);

        // Add history entry
        var history = new IssueStatusHistory
        {
            IssueId = dto.IssueId,
            Status = IssueStatus.Scheduled,
            ChangedAt = DateTime.Now,
            Note = $"Scheduled for {dto.VisitDate:yyyy-MM-dd HH:mm} with worker {dto.WorkerName}."
        };
        await _unitOfWork.StatusHistories.AddAsync(history);

        await _unitOfWork.CompleteAsync();
        return true;
    }
}
