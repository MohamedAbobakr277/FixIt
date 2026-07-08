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
        var totalSystemAdmins = (await _userManager.GetUsersInRoleAsync(AppConstants.SystemAdminRole)).Count;
        var totalAdmins = (await _userManager.GetUsersInRoleAsync(AppConstants.AdminRole)).Count;
        var totalCitizens = (await _userManager.GetUsersInRoleAsync(AppConstants.CitizenRole)).Count;
        var totalUsers = await _userManager.Users.CountAsync();

        var totalIssues = await _context.Issues.CountAsync();
        var activeIssues = await _context.Issues.CountAsync(i => i.Status != FixIt.Common.Enums.IssueStatus.Resolved && i.Status != FixIt.Common.Enums.IssueStatus.Rejected);
        var resolvedIssues = await _context.Issues.CountAsync(i => i.Status == FixIt.Common.Enums.IssueStatus.Resolved);

        return new SystemDashboardStatsDto
        {
            TotalUsers = totalUsers,
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
        // Use LINQ join to avoid N+1 query problem from calling GetRolesAsync in a loop
        var usersWithRoles = await (from user in _context.Users
                                    let roleId = _context.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).FirstOrDefault()
                                    let roleName = _context.Roles.Where(r => r.Id == roleId).Select(r => r.Name).FirstOrDefault()
                                    select new UserManagementDto
                                    {
                                        Id = user.Id,
                                        Email = user.Email ?? "No Email",
                                        FullName = user.FullName,
                                        Role = roleName ?? "No Role",
                                        IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                                        CreatedAt = user.CreatedAt
                                    }).ToListAsync();

        return usersWithRoles.OrderByDescending(u => u.CreatedAt);
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
