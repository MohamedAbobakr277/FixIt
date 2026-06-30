using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using FixIt.Common.DTOs;
using FixIt.Common.Enums;
using FixIt.DAL.Entities;
using FixIt.DAL.UnitOfWork;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FixIt.BLL.Services;

public class AdminProfileService : IAdminProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public AdminProfileService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _environment = environment;
    }

    public async Task<AdminProfileDto> GetAdminProfileAsync(string adminId)
    {
        // FindByIdAsync works for any user type; cast to Admin for Department (may be null for legacy accounts)
        var user = await _userManager.FindByIdAsync(adminId);
        if (user == null) throw new Exception("Admin not found");

        var admin = user as Admin;

        var issues = _unitOfWork.Issues.GetAll();
        var totalIssues = await issues.CountAsync();
        var resolvedIssues = await issues.CountAsync(i =>
            i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed);
        var pendingIssues = await issues.CountAsync(i =>
            i.Status == IssueStatus.New || i.Status == IssueStatus.Approved || i.Status == IssueStatus.InProgress);

        return new AdminProfileDto
        {
            FullName = user.FullName,
            Email = user.Email ?? "",
            IsTwoFactorEnabled = user.TwoFactorEnabled,
            PhoneNumber = user.PhoneNumber,
            Department = admin?.Department,
            MemberSince = user.CreatedAt,
            ProfilePicture = user.ProfileImageUrl,
            TotalIssuesManaged = totalIssues,
            ResolvedIssues = resolvedIssues,
            PendingIssues = pendingIssues,
            EmailIssueUpdates = user.EmailIssueUpdates,
            EmailMaintenanceAlerts = user.EmailMaintenanceAlerts,
            EmailWeeklyReports = user.EmailWeeklyReports,
            AppRealTimePush = user.AppRealTimePush,
            AppDirectMessages = user.AppDirectMessages
        };
    }

    public async Task<bool> UpdateAdminProfileAsync(string adminId, FixIt.BLL.DTOs.UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(adminId);
        if (user == null) return false;

        var typedAdmin = user as Admin;

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim().ToLower();
        user.UserName = dto.Email.Trim().ToLower();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        if (typedAdmin != null)
            typedAdmin.Department = dto.Address?.Trim();

        if (dto.ProfilePicture != null)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, AppConstants.UploadsProfilesPath);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(dto.ProfilePicture.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
                await dto.ProfilePicture.CopyToAsync(fileStream);

            user.ProfileImageUrl = $"/{AppConstants.UploadsProfilesPath}/{uniqueFileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateAdminNotificationsAsync(string adminId, UpdateNotificationsDto dto)
    {
        var admin = await _userManager.FindByIdAsync(adminId) as ApplicationUser;
        if (admin == null) return false;

        admin.EmailIssueUpdates = dto.EmailIssueUpdates;
        admin.EmailMaintenanceAlerts = dto.EmailMaintenanceAlerts;
        admin.EmailWeeklyReports = dto.EmailWeeklyReports;
        admin.AppRealTimePush = dto.AppRealTimePush;
        admin.AppDirectMessages = dto.AppDirectMessages;

        var result = await _userManager.UpdateAsync(admin);
        return result.Succeeded;
    }
}
