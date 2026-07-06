using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using FixIt.DAL.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FixIt.Common.Enums;
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

    // GET /Payment/SetupTest
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> SetupTest()
    {
        var unitOfWork = (FixIt.DAL.UnitOfWork.IUnitOfWork)HttpContext.RequestServices.GetService(typeof(FixIt.DAL.UnitOfWork.IUnitOfWork))!;

        // 1. Create Citizen
        var citizenEmail = "citizen@test.com";
        var citizen = await _userManager.FindByEmailAsync(citizenEmail);
        if (citizen == null)
        {
            citizen = new Citizen
            {
                FullName = "Test Citizen",
                UserName = citizenEmail,
                Email = citizenEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(citizen, "Password123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(citizen, AppConstants.CitizenRole);
            }
        }
        else
        {
            citizen.EmailConfirmed = true;
            await _userManager.UpdateAsync(citizen);
        }

        // 2. Create Admin
        var adminEmail = "admin@test.com";
        var admin = await _userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new Admin
            {
                FullName = "Test Admin",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(admin, "Password123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, AppConstants.AdminRole);
            }
        }
        else
        {
            admin.EmailConfirmed = true;
            await _userManager.UpdateAsync(admin);
        }

        // 3. Create a mock resolved issue
        var issue = await unitOfWork.Issues.GetAll()
            .FirstOrDefaultAsync(i => i.Title == "Stripe Test Issue" && i.CitizenId == citizen.Id);

        if (issue == null)
        {
            issue = new Issue
            {
                Title = "Stripe Test Issue",
                Description = "A test issue to verify the Stripe payment integration flow.",
                Location = "123 Test Street",
                Category = IssueCategory.GeneralMaintenance,
                Status = IssueStatus.Resolved,
                Priority = IssuePriority.Medium,
                SubmittedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CitizenId = citizen.Id,
                AdminId = admin.Id
            };
            await unitOfWork.Issues.AddAsync(issue);
            await unitOfWork.CompleteAsync();

            // Create MaintenanceSchedule
            var schedule = new MaintenanceSchedule
            {
                IssueId = issue.IssueId,
                VisitDate = DateTime.UtcNow.AddDays(1),
                EstimatedCost = 75.00m,
                WorkerName = "Test Technician",
                CreatedAt = DateTime.UtcNow
            };
            await unitOfWork.Schedules.AddAsync(schedule);

            // Create MaintenanceReport
            var report = new MaintenanceReport
            {
                IssueId = issue.IssueId,
                Summary = "The road issue was repaired successfully.",
                WorkerNotes = "Used asphalt filler.",
                AdminId = admin.Id,
                SubmittedAt = DateTime.UtcNow
            };
            await unitOfWork.Reports.AddAsync(report);
            await unitOfWork.CompleteAsync();
        }
        else if (issue.Status != IssueStatus.Resolved)
        {
            issue.Status = IssueStatus.Resolved;
            unitOfWork.Issues.Update(issue);
            await unitOfWork.CompleteAsync();
        }

        // Remove any completed payments for this issue to reset the test
        var payment = await unitOfWork.Payments.GetAll()
            .FirstOrDefaultAsync(p => p.IssueId == issue.IssueId);
        if (payment != null)
        {
            unitOfWork.Payments.Delete(payment);
            await unitOfWork.CompleteAsync();
        }

        return Content($"Test setup successful!\n\n" +
                       $"1. Login credentials:\n" +
                       $"   - Role: Citizen\n" +
                       $"   - Email: {citizenEmail}\n" +
                       $"   - Password: Password123!\n\n" +
                       $"2. Log in, then navigate directly to the resolved issue details page:\n" +
                       $"   http://localhost:5122/Issue/Details/{issue.IssueId}\n\n" +
                       $"3. On that page, you will see the 'Resolution Payment' box with a dynamic cost of 75.00 EGP.\n" +
                       $"   Click '[Dev] Simulate Payment Success' or 'Pay Now with Stripe' to test.", "text/plain");
    }

    // GET /Payment/SimulateSuccess
    [Authorize(Roles = AppConstants.CitizenRole)]
    [HttpGet]
    public async Task<IActionResult> SimulateSuccess(int issueId)
    {
        var citizenId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(citizenId))
        {
            return Challenge();
        }

        var unitOfWork = (FixIt.DAL.UnitOfWork.IUnitOfWork)HttpContext.RequestServices.GetService(typeof(FixIt.DAL.UnitOfWork.IUnitOfWork))!;
        
        var issue = await unitOfWork.Issues.GetByIdAsync(issueId);
        if (issue == null || issue.CitizenId != citizenId)
        {
            return NotFound("Issue not found or unauthorized.");
        }

        var payment = await unitOfWork.Payments.GetAll()
            .FirstOrDefaultAsync(p => p.IssueId == issueId);

        var mockSessionId = $"mock_session_{issueId}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // Load estimated cost if available
        var schedule = await unitOfWork.Schedules.GetAll()
            .FirstOrDefaultAsync(s => s.IssueId == issueId);
        decimal amount = (schedule != null && schedule.EstimatedCost > 0) ? schedule.EstimatedCost : 50.00m;

        if (payment == null)
        {
            payment = new Payment
            {
                IssueId = issueId,
                CitizenId = citizenId,
                Amount = amount,
                Currency = "EGP",
                StripeSessionId = mockSessionId,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await unitOfWork.Payments.AddAsync(payment);
            await unitOfWork.CompleteAsync();
        }
        else
        {
            payment.StripeSessionId = mockSessionId;
            payment.Status = PaymentStatus.Pending;
            unitOfWork.Payments.Update(payment);
            await unitOfWork.CompleteAsync();
        }

        // Simulating completion
        payment.Status = PaymentStatus.Completed;
        payment.StripePaymentIntentId = $"mock_pi_{issueId}";
        payment.CompletedAt = DateTime.UtcNow;
        unitOfWork.Payments.Update(payment);

        // Add success notification
        var notification = new Notification
        {
            UserId = citizenId,
            Title = "Payment Successful (Simulated)",
            Message = $"Payment of {payment.Amount} {payment.Currency} for Issue #{payment.IssueId} was successfully processed.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityUrl = $"/Issue/Details/{payment.IssueId}",
            Type = NotificationType.Push
        };
        await unitOfWork.Notifications.AddAsync(notification);
        await unitOfWork.CompleteAsync();

        return RedirectToAction("Success", new { session_id = mockSessionId });
    }
}
