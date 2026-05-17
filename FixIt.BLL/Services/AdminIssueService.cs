using AutoMapper;
using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Enums;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace FixIt.BLL.Services;

public class AdminIssueService : IAdminIssueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminIssueService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AdminIssueListPageDto> GetIssuesAsync(string? search, IssueStatus? status)
    {
        var query = _unitOfWork.Issues.GetAll(i => i.Citizen);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => i.Title.Contains(search) || i.Description.Contains(search) || i.Location.Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        var issues = await query.OrderByDescending(i => i.SubmittedAt).ToListAsync();

        return new AdminIssueListPageDto
        {
            Items = _mapper.Map<List<AdminIssueListItemDto>>(issues),
            TotalCount = issues.Count,
            SearchTerm = search,
            StatusFilter = status
        };
    }

    public async Task<AdminIssueDetailsDto?> GetIssueDetailsAsync(int issueId)
    {
        var issue = await _unitOfWork.Issues.GetAll(
            i => i.Citizen,
            i => i.MaintenanceSchedule,
            i => i.MaintenanceReport
        )
        .Include(i => i.StatusHistory)
            .ThenInclude(h => h.ChangedBy)
        .FirstOrDefaultAsync(i => i.IssueId == issueId);

        if (issue == null) return null;

        return _mapper.Map<AdminIssueDetailsDto>(issue);
    }

    public async Task<bool> ChangeStatusAsync(int issueId, IssueStatus newStatus, string adminId, string? note = null)
    {
        var issue = await _unitOfWork.Issues.GetByIdAsync(issueId);
        if (issue == null) return false;

        // Create history entry
        var history = new IssueStatusHistory
        {
            IssueId = issueId,
            Status = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedById = adminId,
            Note = note
        };

        issue.Status = newStatus;
        issue.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.StatusHistories.AddAsync(history);
        _unitOfWork.Issues.Update(issue);

        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> SaveAdminNotesAsync(int issueId, string? notes)
    {
        var issue = await _unitOfWork.Issues.GetByIdAsync(issueId);
        if (issue == null) return false;

        issue.AdminNotes = notes;
        issue.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Issues.Update(issue);
        return await _unitOfWork.CompleteAsync() > 0;
    }
}
