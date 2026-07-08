using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FixIt.PL.Areas.SystemAdmin.Controllers;

[Area("SystemAdmin")]
[Route("SystemAdmin/[action]")]
[Authorize(Roles = AppConstants.SystemAdminRole)]
public class SystemAdminController : Controller
{
    private readonly ISystemAdminService _systemAdminService;

    public SystemAdminController(ISystemAdminService systemAdminService)
    {
        _systemAdminService = systemAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var stats = await _systemAdminService.GetSystemDashboardStatsAsync();
        return View(stats);
    }

    [HttpGet]
    public async Task<IActionResult> Users(string searchTerm, string roleFilter)
    {
        ViewData["CurrentSearch"] = searchTerm;
        ViewData["CurrentRole"] = roleFilter;
        
        var users = await _systemAdminService.GetAllUsersAsync(searchTerm, roleFilter);
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Invalid user ID.";
            return RedirectToAction(nameof(Users));
        }

        var success = await _systemAdminService.ToggleUserLockStatusAsync(userId);
        if (success)
        {
            TempData["SuccessMessage"] = "User lock status updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update user lock status.";
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string userId, string newRole)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
        {
            TempData["ErrorMessage"] = "Invalid parameters for role change.";
            return RedirectToAction(nameof(Users));
        }

        var success = await _systemAdminService.ChangeUserRoleAsync(userId, newRole);
        if (success)
        {
            TempData["SuccessMessage"] = $"User role successfully changed to {newRole}.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to change user role. Ensure the role exists and user is valid.";
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> ExportUsers()
    {
        var users = await _systemAdminService.GetAllUsersAsync();
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("ID,FullName,Email,Role,Status,JoinedDate");
        foreach (var u in users)
        {
            var status = u.IsLockedOut ? "Locked" : "Active";
            builder.AppendLine($"{u.Id},{u.FullName},{u.Email},{u.Role},{status},{u.CreatedAt:yyyy-MM-dd}");
        }
        return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"FixIt_Users_{DateTime.Now:yyyyMMdd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> SystemLogs()
    {
        var logs = await _systemAdminService.GetSystemLogsAsync();
        return View(logs);
    }
}
