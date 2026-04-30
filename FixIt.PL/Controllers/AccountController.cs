using FixIt.Common.DTOs;
using FixIt.BLL.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using FixIt.BLL.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FixIt.PL.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IValidator<FixIt.Common.DTOs.RegisterDto> _registerValidator;
    private readonly IValidator<FixIt.Common.DTOs.LoginDto> _loginValidator;

    public AccountController(
        IAccountService accountService,
        IValidator<FixIt.Common.DTOs.RegisterDto> registerValidator,
        IValidator<FixIt.Common.DTOs.LoginDto> loginValidator)
    {
        _accountService = accountService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
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
    public async Task<IActionResult> Register(FixIt.Common.DTOs.RegisterDto dto)
    {
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(dto);
        }

        var errors = await _accountService.RegisterAsync(dto);
        if (errors == null) // null means success
        {
            TempData["SuccessMessage"] = "Account created successfully! Please sign in.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in errors)
            ModelState.AddModelError(string.Empty, error);

        return View(dto);
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
    public async Task<IActionResult> Login(FixIt.Common.DTOs.LoginDto dto, string? returnUrl = null)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            ViewData["ReturnUrl"] = returnUrl;
            return View(dto);
        }

        var result = await _accountService.LoginAsync(dto);
        if (result == null) // null means success
        {
            TempData["SuccessMessage"] = "Welcome back!";
            return RedirectToLocal(returnUrl);
        }

        if (result == "REQUIRES_2FA")
        {
            // Store email in session to verify 2FA later
            HttpContext.Session.SetString("TwoFactorEmail", dto.Email);
            HttpContext.Session.SetString("TwoFactorRememberMe", dto.RememberMe.ToString());
            if (!string.IsNullOrEmpty(returnUrl))
                HttpContext.Session.SetString("ReturnUrl", returnUrl);
            return RedirectToAction(nameof(Verify2FA));
        }

        ModelState.AddModelError(string.Empty, result);
        ViewData["ReturnUrl"] = returnUrl;
        return View(dto);
    }

    // ── GET /Account/Verify2FA ──────────────────────────────────────────────
    [HttpGet]
    public IActionResult Verify2FA()
    {
        var email = HttpContext.Session.GetString("TwoFactorEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(Login));

        return View(new TwoFactorLoginDto { Email = email });
    }

    // ── POST /Account/Verify2FA ─────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify2FA(TwoFactorLoginDto dto)
    {
        var email = HttpContext.Session.GetString("TwoFactorEmail");
        var rememberMeStr = HttpContext.Session.GetString("TwoFactorRememberMe");
        var rememberMe = bool.TryParse(rememberMeStr, out var rm) && rm;
        
        if (string.IsNullOrEmpty(email)) return RedirectToAction(nameof(Login));

        var isValid = await _accountService.VerifyTwoFactorTokenByEmailAsync(email, dto.Code, rememberMe);
        
        if (isValid)
        {
            HttpContext.Session.Remove("TwoFactorEmail");
            HttpContext.Session.Remove("TwoFactorRememberMe");
            var returnUrl = HttpContext.Session.GetString("ReturnUrl");
            HttpContext.Session.Remove("ReturnUrl");
            return RedirectToLocal(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid verification code.");
        return View(dto);
    }

    // ── GET /Account/Setup2FA ───────────────────────────────────────────────
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Setup2FA()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var isEnabled = await _accountService.IsTwoFactorEnabledAsync(userId);
        if (isEnabled)
        {
            return View("TwoFactorEnabled");
        }

        var setupInfo = await _accountService.GenerateTwoFactorSetupAsync(userId);
        return View(setupInfo);
    }

    // ── POST /Account/Setup2FA ──────────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup2FA(string code)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var success = await _accountService.EnableTwoFactorAsync(userId, code);
        if (success)
        {
            TempData["SuccessMessage"] = "2FA enabled successfully! Please save your recovery codes.";
            return RedirectToAction(nameof(RecoveryCodes));
        }

        ModelState.AddModelError(string.Empty, "Invalid code. Please try again.");
        var setupInfo = await _accountService.GenerateTwoFactorSetupAsync(userId);
        return View(setupInfo);
    }

    // ── POST /Account/Disable2FA ───────────────────────────────────────────
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable2FA()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        await _accountService.DisableTwoFactorAsync(userId);
        TempData["SuccessMessage"] = "2FA has been disabled.";
        return RedirectToAction("Index", "Home");
    }

    // ── GET /Account/RecoveryCodes ─────────────────────────────────────────
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> RecoveryCodes()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var codes = await _accountService.GetRecoveryCodesAsync(userId);
        if (codes == null || !codes.Any())
        {
            // Generate new codes if none exist
            codes = await _accountService.GenerateRecoveryCodesAsync(userId);
        }
        return View(new TwoFactorRecoveryDto { RecoveryCodes = codes });
    }

    // ── GET /Account/RecoveryCodeLogin ─────────────────────────────────────
    [HttpGet]
    public IActionResult RecoveryCodeLogin()
    {
        var email = HttpContext.Session.GetString("TwoFactorEmail");
        if (string.IsNullOrEmpty(email)) return RedirectToAction(nameof(Login));

        return View(new TwoFactorLoginDto { Email = email });
    }

    // ── POST /Account/RecoveryCodeLogin ────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecoveryCodeLogin(TwoFactorLoginDto dto)
    {
        var email = HttpContext.Session.GetString("TwoFactorEmail");
        var rememberMeStr = HttpContext.Session.GetString("TwoFactorRememberMe");
        var rememberMe = bool.TryParse(rememberMeStr, out var rm) && rm;
        
        if (string.IsNullOrEmpty(email)) return RedirectToAction(nameof(Login));

        var user = await _accountService.GetUserByEmailAsync(email);
        if (user == null) return RedirectToAction(nameof(Login));

        var isValid = await _accountService.RedeemRecoveryCodeAsync(user.Id, dto.Code, rememberMe);
        
        if (isValid)
        {
            HttpContext.Session.Remove("TwoFactorEmail");
            HttpContext.Session.Remove("TwoFactorRememberMe");
            var returnUrl = HttpContext.Session.GetString("ReturnUrl");
            HttpContext.Session.Remove("ReturnUrl");
            return RedirectToLocal(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid recovery code.");
        return View(dto);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
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
