using System.Security.Claims;
using FixIt.Common.DTOs;
using FixIt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace FixIt.PL.Controllers;

[Authorize(Roles = "Citizen")] // Assuming rating is strictly for citizens
public class RatingController : Controller
{
    private readonly IRatingService _ratingService;
    private readonly IValidator<CreateRatingDto> _validator;

    public RatingController(IRatingService ratingService, IValidator<CreateRatingDto> validator)
    {
        _ratingService = ratingService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int issueId)
    {
        var citizenId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(citizenId))
        {
            return Unauthorized();
        }

        var (canRate, errorMessage) = await _ratingService.CanRateIssueAsync(issueId, citizenId);
        if (!canRate)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction("Index", "Home"); // Fallback to Home since Issue/Index might not exist
        }

        var model = new CreateRatingDto { IssueId = issueId, Stars = 5 };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRatingDto model)
    {
        var validationResult = await _validator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(model);
        }

        try
        {
            var citizenId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(citizenId))
            {
                return Unauthorized();
            }

            await _ratingService.CreateRatingAsync(model, citizenId);
            
            TempData["SuccessMessage"] = "Thank you! Your rating has been submitted successfully.";
            return RedirectToAction("Details", "Issue", new { id = model.IssueId });
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Issue");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Issue", new { id = model.IssueId });
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An unexpected error occurred while saving your rating.");
            return View(model);
        }
    }
}
