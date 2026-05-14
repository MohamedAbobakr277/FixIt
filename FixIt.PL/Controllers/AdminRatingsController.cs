using FixIt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FixIt.PL.Controllers;

[Authorize(Roles = "Admin")]
public class AdminRatingsController : Controller
{
    private readonly IRatingService _ratingService;

    public AdminRatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    public async Task<IActionResult> Index()
    {
        var ratings = await _ratingService.GetAllRatingsAsync();
        return View(ratings);
    }

    public async Task<IActionResult> Details(int id)
    {
        var rating = await _ratingService.GetRatingByIdAsync(id);
        if (rating == null)
        {
            return NotFound();
        }

        return View(rating);
    }
}
