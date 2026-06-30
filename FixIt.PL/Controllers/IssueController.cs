using Microsoft.AspNetCore.Mvc;
using FixIt.BLL.Services;
using FixIt.BLL.Interfaces;
using FixIt.BLL.DTOs;
using FixIt.Common.DTOs;
using FixIt.Common.Constants;
using FixIt.Common.Enums;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using System.Security.Claims;
using System.Text.Json;

namespace FixIt.PL.Controllers;

[Authorize(Roles = AppConstants.CitizenRole + "," + AppConstants.AdminRole)]
public class IssueController : Controller
{
    private readonly IIssueService _issueService;
    private readonly IIssueDetailsService _issueDetailsService;
    private readonly IAdminIssueService _adminIssueService;
    private readonly IAiClassificationService _aiClassificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<FixIt.BLL.DTOs.CreateIssueDto> _createIssueValidator;

    public IssueController(
        IIssueService issueService,
        IIssueDetailsService issueDetailsService,
        IAdminIssueService adminIssueService,
        IAiClassificationService aiClassificationService,
        UserManager<ApplicationUser> userManager,
        IValidator<FixIt.BLL.DTOs.CreateIssueDto> createIssueValidator)
    {
        _issueService = issueService;
        _issueDetailsService = issueDetailsService;
        _adminIssueService = adminIssueService;
        _aiClassificationService = aiClassificationService;
        _userManager = userManager;
        _createIssueValidator = createIssueValidator;
    }

    // ─────────────────────────────── CITIZEN ───────────────────────────────

    [HttpGet]
    [Authorize(Roles = AppConstants.CitizenRole)]
    public async Task<IActionResult> Index([FromQuery] FixIt.Common.DTOs.IssueFilterDto filter)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var paginatedIssues = await _issueService.GetCitizenIssuesAsync(user.Id, filter);

        ViewData["Filter"] = filter;
        return View(paginatedIssues);
    }

    [HttpGet]
    [Authorize(Roles = AppConstants.CitizenRole)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.CitizenRole)]
    public async Task<IActionResult> Create(FixIt.BLL.DTOs.CreateIssueDto dto)
    {
        var validationResult = await _createIssueValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(dto);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            var issueId = await _issueService.CreateAsync(dto, user.Id);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Image", ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.CitizenRole)]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ClassifyFromImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest(new { error = "No image uploaded." });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(image.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { error = "Invalid image format. Use JPG, PNG, or WebP." });
        }

        // Read image bytes
        using var ms = new MemoryStream();
        await image.CopyToAsync(ms);
        var imageBytes = ms.ToArray();

        var result = await _aiClassificationService.ClassifyIssueFromImageAsync(
            imageBytes, image.ContentType);

        if (result == null)
        {
            return StatusCode(503, new { error = "AI classification service is currently unavailable." });
        }

        return Json(new
        {
            title = result.SuggestedTitle,
            description = result.SuggestedDescription,
            category = result.SuggestedCategory.ToString(),
            categoryValue = (int)result.SuggestedCategory,
            priority = result.SuggestedPriority.ToString(),
            priorityValue = (int)result.SuggestedPriority,
            categoryReason = result.CategoryReason,
            priorityReason = result.PriorityReason,
            confidence = result.Confidence
        });
    }

    [Authorize(Roles = AppConstants.CitizenRole)]
    public async Task<IActionResult> Details(int id)
    {
        var issue = await _issueDetailsService.GetIssueDetailsAsync(id);

        if (issue == null)
        {
            return NotFound();
        }

        return View(issue);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(AddCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid comment.";
            return User.IsInRole(AppConstants.AdminRole) 
                ? RedirectToAction(nameof(AdminDetails), new { id = dto.IssueId }) 
                : RedirectToAction(nameof(Details), new { id = dto.IssueId });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        try
        {
            await _issueDetailsService.AddCommentAsync(dto.IssueId, userId, dto.Text);
            TempData["SuccessMessage"] = "Comment added successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Failed to add comment.";
        }

        return User.IsInRole(AppConstants.AdminRole) 
            ? RedirectToAction(nameof(AdminDetails), new { id = dto.IssueId }) 
            : RedirectToAction(nameof(Details), new { id = dto.IssueId });
    }

    // ─────────────────────────────── ADMIN ─────────────────────────────────

    [HttpGet]
    [Authorize(Roles = AppConstants.AdminRole)]
    public async Task<IActionResult> AdminIndex(string? search, IssueStatus? status)
    {
        var page = await _adminIssueService.GetIssuesAsync(search, status);
        return View(page);
    }

    [HttpGet]
    [Authorize(Roles = AppConstants.AdminRole)]
    public async Task<IActionResult> AdminDetails(int id)
    {
        var details = await _adminIssueService.GetIssueDetailsAsync(id);
        if (details == null) return NotFound();
        return View(details);
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAdminNotes(int id, string? adminNotes)
    {
        var ok = await _adminIssueService.SaveAdminNotesAsync(id, adminNotes);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
            ok ? "Admin notes saved." : "Failed to save admin notes.";
        return RedirectToAction(nameof(AdminDetails), new { id });
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, IssueStatus newStatus, string? note)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminId)) return Challenge();

        var ok = await _adminIssueService.ChangeStatusAsync(id, newStatus, adminId, note);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] =
            ok ? $"Issue moved to {newStatus}." : "Status change not allowed.";
        return RedirectToAction(nameof(AdminDetails), new { id });
    }
}
