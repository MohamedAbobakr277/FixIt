using System.Threading.Tasks;
using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.BLL.Validators;
using FixIt.Common.Constants;
using FixIt.DAL.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FixIt.PL.Controllers;

[Authorize(Roles = AppConstants.CitizenRole)]
public class IssueController : Controller
{
    private readonly IIssueService _issueService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<CreateIssueDto> _createIssueValidator;

    public IssueController(IIssueService issueService, UserManager<ApplicationUser> userManager, IValidator<CreateIssueDto> createIssueValidator)
    {
        _issueService = issueService;
        _userManager = userManager;
        _createIssueValidator = createIssueValidator;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] IssueFilterDto filter)
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
    public async Task<IActionResult> Create(CreateIssueDto dto)
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

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var issue = await _issueService.GetIssueByIdAsync(id, user.Id);
        if (issue == null) return NotFound();

        return View(issue);
    }
}
