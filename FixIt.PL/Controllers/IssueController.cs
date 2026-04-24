using Microsoft.AspNetCore.Mvc;
using FixIt.BLL.Services;
using FixIt.Common.DTOs;

namespace FixIt.PL.Controllers;

public class IssueController : Controller
{
    private readonly IIssueDetailsService _issueDetailsService;

    public IssueController(IIssueDetailsService issueDetailsService)
    {
        _issueDetailsService = issueDetailsService;
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
