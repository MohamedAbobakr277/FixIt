using Microsoft.AspNetCore.Mvc;
using FixIt.BLL.Services;
using FixIt.BLL.Interfaces;
using FixIt.BLL.DTOs;
using FixIt.Common.DTOs;
using FixIt.Common.Constants;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;

namespace FixIt.PL.Controllers;

[Authorize(Roles = AppConstants.CitizenRole + "," + AppConstants.AdminRole)]
public class IssueController : Controller
{
    private readonly IIssueService _issueService;
    private readonly IIssueDetailsService _issueDetailsService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<FixIt.BLL.DTOs.CreateIssueDto> _createIssueValidator;

    public IssueController(IIssueService issueService, IIssueDetailsService issueDetailsService, UserManager<ApplicationUser> userManager, IValidator<FixIt.BLL.DTOs.CreateIssueDto> createIssueValidator)
    {
        _issueService = issueService;
        _issueDetailsService = issueDetailsService;
        _userManager = userManager;
        _createIssueValidator = createIssueValidator;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] FixIt.Common.DTOs.IssueFilterDto filter)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var paginatedIssues = await _issueService.GetCitizenIssuesAsync(user.Id, filter);

        ViewData["Filter"] = filter;
        return View(paginatedIssues);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
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

    public async Task<IActionResult> Details(int id)
    {
        var issue = await _issueDetailsService.GetIssueDetailsAsync(id);
        
        if (issue == null)
        {
            return NotFound();
        }

        return View(issue);
    }
}
