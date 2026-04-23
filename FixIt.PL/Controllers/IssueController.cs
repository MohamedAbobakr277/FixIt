using System.Threading.Tasks;
using FixIt.BLL.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FixIt.PL.Controllers;

[Authorize(Roles = AppConstants.CitizenRole)]
public class IssueController : Controller
{
    private readonly IIssueService _issueService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IssueController(IIssueService issueService, UserManager<ApplicationUser> userManager)
    {
        _issueService = issueService;
        _userManager = userManager;
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
}
