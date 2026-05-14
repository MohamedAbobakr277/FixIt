using FixIt.BLL.DTOs;
using FixIt.Common.Enums;

namespace FixIt.BLL.Interfaces;

public interface IAdminIssueService
{
    Task<AdminIssueListPageDto> GetIssuesAsync(string? search, IssueStatus? status);
    Task<AdminIssueDetailsDto?> GetIssueDetailsAsync(int issueId);
    Task<bool> ChangeStatusAsync(int issueId, IssueStatus newStatus, string adminId, string? note = null);
    Task<bool> SaveAdminNotesAsync(int issueId, string? notes);
}
