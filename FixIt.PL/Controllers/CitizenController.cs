using FixIt.BLL.Interfaces;
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
}
