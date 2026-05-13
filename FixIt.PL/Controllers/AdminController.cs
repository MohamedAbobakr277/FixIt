using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FixIt.BLL.Interfaces;
using FixIt.BLL.DTOs;
using FluentValidation;
using System.Security.Claims;

namespace FixIt.PL.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly IScheduleService _scheduleService;
    private readonly IReportService _reportService;
    private readonly IValidator<CreateScheduleDto> _scheduleValidator;
    private readonly IValidator<CreateReportDto> _reportValidator;

    public AdminController(
        IAdminDashboardService dashboardService,
        IScheduleService scheduleService,
        IReportService reportService,
        IValidator<CreateScheduleDto> scheduleValidator,
        IValidator<CreateReportDto> reportValidator)
    {
        _dashboardService = dashboardService;
        _scheduleService = scheduleService;
        _reportService = reportService;
        _scheduleValidator = scheduleValidator;
        _reportValidator = reportValidator;
    }

    public async Task<IActionResult> Dashboard()
    {
        var stats = await _dashboardService.GetDashboardStatsAsync();
        return View(stats);
    }

    // ── GET /Admin/Schedule ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Schedule()
    {
        var model = await _scheduleService.GetSchedulePageAsync();
        return View(model);
    }

    // ── GET /Admin/CreateSchedule/{id} ─────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> CreateSchedule(int id)
    {
        var model = await _scheduleService.GetCreateScheduleDtoAsync(id);
        if (model == null)
        {
            TempData["ErrorMessage"] = "Issue not found or cannot be scheduled.";
            return RedirectToAction("Schedule");
        }
        return View(model);
    }

    // ── POST /Admin/CreateSchedule ────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSchedule(CreateScheduleDto dto)
    {
        var validation = await _scheduleValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(dto);
        }

        var success = await _scheduleService.CreateScheduleAsync(dto);
        if (success)
        {
            TempData["SuccessMessage"] = "Maintenance scheduled successfully!";
            return RedirectToAction(nameof(Schedule));
        }

        TempData["ErrorMessage"] = "Failed to create schedule. Issue may not be approved.";
        return View(dto);
    }

    // ── GET /Admin/Reports ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Reports(int? issueId)
    {
        var model = await _reportService.GetReportsPageAsync();
        
        // Pre-select issue if provided
        if (issueId.HasValue)
        {
            var issue = model.InProgressIssues.FirstOrDefault(i => i.IssueId == issueId.Value);
            if (issue != null)
            {
                ViewBag.SelectedIssueId = issueId.Value;
            }
        }
        
        return View(model);
    }

    // ── GET /Admin/CreateReport/{id} ───────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> CreateReport(int id)
    {
        var model = await _reportService.GetCreateReportDtoAsync(id);
        if (model == null)
        {
            TempData["ErrorMessage"] = "Issue not found or not in progress.";
            return RedirectToAction("Reports");
        }
        return View(model);
    }

    // ── POST /Admin/SubmitReport ────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReport(CreateReportDto dto)
    {
        var validation = await _reportValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View("CreateReport", dto);
        }

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminId))
        {
            return Challenge();
        }

        var success = await _reportService.SubmitReportAsync(dto, adminId);
        if (success)
        {
            TempData["SuccessMessage"] = "Report submitted successfully! Issue marked as resolved.";
            return RedirectToAction(nameof(Reports));
        }

        TempData["ErrorMessage"] = "Failed to submit report. Issue may not be in progress.";
        return View("CreateReport", dto);
    }
}
