using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FixIt.PL.Controllers;

public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentController(
        IPaymentService paymentService,
        UserManager<ApplicationUser> userManager)
    {
        _paymentService = paymentService;
        _userManager = userManager;
    }

    // GET /Payment/Checkout/{issueId}
    [Authorize(Roles = AppConstants.CitizenRole)]
    [HttpGet]
    public async Task<IActionResult> Checkout(int id)
    {
        var citizenId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(citizenId))
        {
            return Challenge();
        }

        try
        {
            var checkoutUrl = await _paymentService.CreateCheckoutSessionAsync(id, citizenId);
            return Redirect(checkoutUrl);
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Issue", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Issue", new { id });
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while initiating the payment process. Please try again.";
            return RedirectToAction("Details", "Issue", new { id });
        }
    }

    // GET /Payment/Success
    [Authorize(Roles = AppConstants.CitizenRole)]
    [HttpGet]
    public async Task<IActionResult> Success(string session_id)
    {
        if (string.IsNullOrEmpty(session_id))
        {
            TempData["ErrorMessage"] = "Invalid payment session details.";
            return RedirectToAction("Index", "Issue");
        }

        var payment = await _paymentService.GetPaymentBySessionIdAsync(session_id);
        if (payment == null)
        {
            TempData["ErrorMessage"] = "Payment record not found.";
            return RedirectToAction("Index", "Issue");
        }

        var citizenId = _userManager.GetUserId(User);
        if (payment.CitizenId != citizenId)
        {
            return Forbid();
        }

        return View(payment);
    }

    // GET /Payment/Cancel
    [Authorize(Roles = AppConstants.CitizenRole)]
    [HttpGet]
    public IActionResult Cancel(int issueId)
    {
        ViewBag.IssueId = issueId;
        return View();
    }

    // POST /api/Payment/Webhook
    [HttpPost]
    [IgnoreAntiforgeryToken]
    [Route("api/Payment/Webhook")]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signatureHeader = Request.Headers["Stripe-Signature"];

        if (string.IsNullOrEmpty(signatureHeader))
        {
            return BadRequest("Missing Stripe-Signature header.");
        }

        var processed = await _paymentService.HandleWebhookAsync(json, signatureHeader.ToString());
        if (processed)
        {
            return Ok();
        }

        // Return 200/Ok or 400 depending on signature validation. 
        // If validation failed or signature invalid, it returns false inside. 
        // But Stripe recommends returning 400 for bad signatures so they can troubleshoot.
        return BadRequest("Webhook event not processed or signature invalid.");
    }
}
