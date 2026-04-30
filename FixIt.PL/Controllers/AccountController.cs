using FixIt.Common.DTOs;
using FixIt.BLL.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using FixIt.BLL.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Authorization;

namespace FixIt.PL.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IEmailSenderService _emailSender;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        IAccountService accountService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IEmailSenderService emailSender,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _accountService = accountService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _emailSender = emailSender;
        _signInManager = signInManager;
        _userManager = userManager;
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

        var (errors, userId, token) = await _accountService.RegisterAsync(dto);
        if (errors == null && userId != null && token != null) // success
        {
            // Encode the token so special chars (+, /, =) survive URL transmission
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = userId, code = encodedToken },
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

        // Decode the token back before passing to Identity
        var decodedCode = System.Net.WebUtility.UrlDecode(code);
        var isConfirmed = await _accountService.ConfirmEmailAsync(userId, decodedCode);
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

    // ── POST /Account/ExternalLogin ────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    // ── GET /Account/ExternalLoginCallback ─────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["ErrorMessage"] = "Error loading external login information.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            TempData["SuccessMessage"] = "Welcome back!";
            return LocalRedirect(returnUrl);
        }

        if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email!);
            
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(nameof(Login));
                }
            }

            var linkResult = await _userManager.AddLoginAsync(user, info);
            if (!linkResult.Succeeded)
            {
                foreach (var error in linkResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(nameof(Login));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["SuccessMessage"] = "Account linked and successfully logged in.";
            return LocalRedirect(returnUrl);
        }

        TempData["ErrorMessage"] = "Email claim not received from: " + info.LoginProvider;
        return RedirectToAction(nameof(Login), new { returnUrl });
    }
}
