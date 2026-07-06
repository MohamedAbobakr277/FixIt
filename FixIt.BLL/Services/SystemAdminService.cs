using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using FixIt.DAL.Data;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FixIt.BLL.Services;

public class SystemAdminService : ISystemAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly FixItDbContext _context;

    public SystemAdminService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        FixItDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<SystemDashboardStatsDto> GetSystemDashboardStatsAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        
        int totalCitizens = 0;
        int totalAdmins = 0;
        int totalSystemAdmins = 0;

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(AppConstants.SystemAdminRole))
                totalSystemAdmins++;
            else if (roles.Contains(AppConstants.AdminRole))
                totalAdmins++;
            else if (roles.Contains(AppConstants.CitizenRole))
                totalCitizens++;
        }

        var totalIssues = await _context.Issues.CountAsync();
        var activeIssues = await _context.Issues.CountAsync(i => i.Status != FixIt.Common.Enums.IssueStatus.Resolved && i.Status != FixIt.Common.Enums.IssueStatus.Rejected);
        var resolvedIssues = await _context.Issues.CountAsync(i => i.Status == FixIt.Common.Enums.IssueStatus.Resolved);

        return new SystemDashboardStatsDto
        {
            TotalUsers = users.Count,
            TotalCitizens = totalCitizens,
            TotalAdmins = totalAdmins,
            TotalSystemAdmins = totalSystemAdmins,
            TotalIssues = totalIssues,
            ActiveIssues = activeIssues,
            ResolvedIssues = resolvedIssues
        };
    }

    public async Task<IEnumerable<UserManagementDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserManagementDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var mainRole = roles.FirstOrDefault() ?? "No Role";
            
            userDtos.Add(new UserManagementDto
            {
                Id = user.Id,
                Email = user.Email ?? "No Email",
                FullName = user.FullName,
                Role = mainRole,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                CreatedAt = user.CreatedAt
            });
        }

        return userDtos.OrderByDescending(u => u.CreatedAt);
    }

    public async Task<bool> ToggleUserLockStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var isLocked = await _userManager.IsLockedOutAsync(user);
        if (isLocked)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            // Lock out for 100 years
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }

        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (!await _roleManager.RoleExistsAsync(newRole))
            return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        
        // Remove existing roles
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return false;

        // Add new role
        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        return addResult.Succeeded;
    }
}
