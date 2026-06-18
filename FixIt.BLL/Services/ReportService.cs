using FixIt.BLL.Interfaces;
using FixIt.BLL.DTOs;
using FixIt.DAL.Entities;
using FixIt.DAL.Repositories;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using FixIt.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace FixIt.BLL.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReportsPageDto> GetReportsPageAsync()
    {
        var inProgressIssues = await _unitOfWork.Issues
            .GetAll(i => i.MaintenanceSchedule)
            .Where(i => i.Status == IssueStatus.InProgress)
            .ToListAsync();

        return new ReportsPageDto
        {
            InProgressIssues = inProgressIssues.Select(i => new InProgressIssueDto
            {
                IssueId = i.IssueId,
                Title = i.Title,
                Location = i.Location,
                WorkerName = i.MaintenanceSchedule?.WorkerName,
                Category = i.Category.ToString()
            }).ToList()
        };
    }

    public async Task<CreateReportDto?> GetCreateReportDtoAsync(int issueId)
    {
        var issue = await _unitOfWork.Issues
            .GetByIdAsync(issueId);

        if (issue == null || issue.Status != IssueStatus.InProgress)
            return null;

        return new CreateReportDto
        {
            IssueId = issueId
        };
    }

    public async Task<bool> SubmitReportAsync(CreateReportDto dto, string adminId)
    {
        var issue = await _unitOfWork.Issues
            .GetByIdAsync(dto.IssueId);

        if (issue == null || issue.Status != IssueStatus.InProgress)
            return false;

        // Handle image uploads
        string? beforeImageUrl = null;
        string? afterImageUrl = null;

        if (dto.BeforeImage != null)
        {
            beforeImageUrl = await SaveImageAsync(dto.BeforeImage, "before");
        }

        if (dto.AfterImage != null)
        {
            afterImageUrl = await SaveImageAsync(dto.AfterImage, "after");
        }

        var report = new MaintenanceReport
        {
            IssueId = dto.IssueId,
            Summary = dto.Summary,
            WorkerNotes = dto.WorkerNotes,
            BeforeImageUrl = beforeImageUrl,
            AfterImageUrl = afterImageUrl,
            SubmittedAt = DateTime.Now,
            AdminId = adminId
        };

        await _unitOfWork.Reports.AddAsync(report);
        
        // Update issue status to Resolved
        issue.Status = IssueStatus.Resolved;
        issue.UpdatedAt = DateTime.Now;
        _unitOfWork.Issues.Update(issue);

        // Add history entry
        var history = new IssueStatusHistory
        {
            IssueId = dto.IssueId,
            Status = IssueStatus.Resolved,
            ChangedAt = DateTime.Now,
            ChangedById = adminId,
            Note = "Maintenance report submitted."
        };
        await _unitOfWork.StatusHistories.AddAsync(history);

        await _unitOfWork.CompleteAsync();
        return true;
    }

    private async Task<string> SaveImageAsync(IFormFile image, string prefix)
    {
        var uploadsFolder = Path.Combine("wwwroot", "uploads", "reports");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{prefix}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(fileStream);
        }

        return Path.Combine("uploads", "reports", uniqueFileName).Replace("\\", "/");
    }
}
