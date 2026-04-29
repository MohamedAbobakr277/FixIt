using FixIt.Common.DTOs;
using FixIt.BLL.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FixIt.PL.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IEmailSenderService _emailSender;

    public AccountController(
        IAccountService accountService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IEmailSenderService emailSender)
    {
        _accountService = accountService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _emailSender = emailSender;
    }

    // ── GET /Account/Register ──────────────────────────────────────────────
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterDto());
    }

    // ── POST /Account/Register ─────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(dto);
        }

        var (errors, userId, token) = await _accountService.RegisterAsync(dto);
        if (errors == null && userId != null && token != null) // success
        {
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = userId, code = token },
                protocol: Request.Scheme);

            if (callbackUrl != null)
            {
                var htmlMessage = $@"
                    <h2>Welcome to FixIt!</h2>
                    <p>Please confirm your email address to activate your account by clicking the link below:</p>
                    <p><a href='{System.Text.Encodings.Web.HtmlEncoder.Default.Encode(callbackUrl)}'>Confirm My Account</a></p>
                    <br/>
                    <p>Or copy and paste this link into your browser:</p>
                    <p>{callbackUrl}</p>";

                await _emailSender.SendEmailAsync(dto.Email, "Confirm your email address - FixIt", htmlMessage);
            }

            return RedirectToAction(nameof(RegisterConfirmation));
        }

        foreach (var error in errors!)
            ModelState.AddModelError(string.Empty, error);

        return View(dto);
    }

    // ── GET /Account/RegisterConfirmation ──────────────────────────────────
    [HttpGet]
    public IActionResult RegisterConfirmation()
    {
        return View();
    }

    // ── GET /Account/ConfirmEmail ──────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var isConfirmed = await _accountService.ConfirmEmailAsync(userId, code);
        ViewData["IsConfirmed"] = isConfirmed;

        return View();
    }

    // ── GET /Account/Login ─────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDto());
    }

    // ── POST /Account/Login ────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            ViewData["ReturnUrl"] = returnUrl;
            return View(dto);
        }

        var loginError = await _accountService.LoginAsync(dto);
        if (loginError == null) // null means success
        {
            TempData["SuccessMessage"] = "Welcome back!";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, loginError);
        ViewData["ReturnUrl"] = returnUrl;
        return View(dto);
    }

    // ── POST /Account/Logout ───────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        TempData["SuccessMessage"] = "You have been successfully logged out.";
        return RedirectToAction("Index", "Home");
    }

    // ── GET /Account/AccessDenied ──────────────────────────────────────────
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
