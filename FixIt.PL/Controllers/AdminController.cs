using FixIt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FixIt.PL.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var stats = await _dashboardService.GetDashboardStatsAsync();
        return View(stats);
    }
}
