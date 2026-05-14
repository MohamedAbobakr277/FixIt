using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FixIt.PL.Models;

namespace FixIt.PL.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Dashboard", "Admin");
            
            if (User.IsInRole("Citizen"))
                return RedirectToAction("Dashboard", "Citizen");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
