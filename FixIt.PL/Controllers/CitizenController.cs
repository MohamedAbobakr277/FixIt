using FixIt.BLL.Interfaces;
using FixIt.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FixIt.PL.Controllers;

[Authorize(Roles = "Citizen")]
public class CitizenController : Controller
{
    private readonly ICitizenDashboardService _dashboardService;

    public CitizenController(ICitizenDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var data = await _dashboardService.GetDashboardDataAsync(userId);
        return View(data);
    }

    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var data = await _dashboardService.GetProfileDataAsync(userId);
        return View(data);
    }

    public async Task<IActionResult> Settings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var data = await _dashboardService.GetProfileDataAsync(userId);
        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> Settings(FixIt.BLL.DTOs.UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var currentData = await _dashboardService.GetProfileDataAsync(userId);
            return View(currentData);
        }

        var citizenId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var success = await _dashboardService.UpdateProfileAsync(citizenId, dto);

        if (success)
        {
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Settings));
        }

        ModelState.AddModelError("", "An error occurred while updating your profile.");
        var profileData = await _dashboardService.GetProfileDataAsync(citizenId);
        return View(profileData);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateNotifications(FixIt.BLL.DTOs.UpdateNotificationsDto dto)
    {
        var citizenId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(citizenId)) return Challenge();

        var success = await _dashboardService.UpdateNotificationsAsync(citizenId, dto);

        if (success)
        {
            TempData["SuccessMessage"] = "Notification preferences updated successfully!";
            // Redirect with a flag to show notifications tab active
            TempData["ActiveTab"] = "notifications";
            return RedirectToAction(nameof(Settings));
        }

        TempData["ErrorMessage"] = "An error occurred while saving notification preferences.";
        return RedirectToAction(nameof(Settings));
    }
}
