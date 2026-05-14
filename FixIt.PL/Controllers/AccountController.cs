using System.Threading.Tasks;
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
    private readonly IValidator<ForgotPasswordDto> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
    private readonly IEmailSenderService _emailSender;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        IAccountService accountService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<ForgotPasswordDto> forgotPasswordValidator,
        IValidator<ResetPasswordDto> resetPasswordValidator,
        IEmailSenderService emailSender,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _accountService = accountService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
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
        if (errors == null && userId != null && token != null)
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
                var htmlMessage = $@"<h2>Welcome to FixIt!</h2><p>Please confirm your email by clicking <a href='{callbackUrl}'>here</a>.</p>";
                await _emailSender.SendEmailAsync(dto.Email, "Confirm your email", htmlMessage);
            }
            return RedirectToAction(nameof(RegisterConfirmation));
        }
        foreach (var error in errors!) ModelState.AddModelError(string.Empty, error);
        return View(dto);
    }

    // ── GET /Account/RegisterConfirmation ──────────────────────────────────
    [HttpGet]
    public IActionResult RegisterConfirmation() => View();

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
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Dashboard", "Admin");
            return RedirectToAction("Index", "Home");
        }

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
            foreach (var error in validation.Errors) ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
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

    // ── GET /Account/ForgotPassword ────────────────────────────────────────
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordDto());
    }

    // ── POST /Account/ForgotPassword ───────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var validation = await _forgotPasswordValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(dto);
        }

        var (success, token, userId) = await _accountService.ForgotPasswordAsync(dto.Email);
        if (success && token != null && userId != null)
        {
            var encoded = System.Net.WebUtility.UrlEncode(token);
            var url = Url.Action("ResetPassword", "Account", new { userId = userId, code = encoded }, Request.Scheme);
            var html = $"<p><a href='{url}'>Click here to reset your password</a></p>";
            await _emailSender.SendEmailAsync(dto.Email, "Reset your password", html);
        }
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    // ── GET /Account/ForgotPasswordConfirmation ────────────────────────────────
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    // ── GET /Account/ResetPassword ──────────────────────────────────────────────
    [HttpGet]
    public IActionResult ResetPassword(string userId, string code)
    {
        var model = new ResetPasswordDto { UserId = userId, Token = code };
        return View(model);
    }

    // ── POST /Account/ResetPassword ─────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var validation = await _resetPasswordValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(dto);
        }

        var decodedToken = System.Net.WebUtility.UrlDecode(dto.Token);
        var result = await _accountService.ResetPasswordAsync(dto.UserId, decodedToken, dto.NewPassword);
        if (result)
        {
            TempData["SuccessMessage"] = "Your password has been reset successfully.";
            return RedirectToAction("Login");
        }
        ModelState.AddModelError(string.Empty, "Invalid token or user.");
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

        // Admin accounts go directly to the Admin Dashboard
        if (User.IsInRole("Admin"))
            return RedirectToAction("Dashboard", "Admin");

        // Citizen accounts go to Citizen Dashboard
        if (User.IsInRole("Citizen"))
            return RedirectToAction("Dashboard", "Citizen");

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

            var existingLogins = await _userManager.GetLoginsAsync(user);

            bool alreadyLinked = existingLogins.Any(l =>
                l.LoginProvider == info.LoginProvider &&
                l.ProviderKey == info.ProviderKey);

            if (!alreadyLinked)
            {
                var linkResult = await _userManager.AddLoginAsync(user, info);

                if (!linkResult.Succeeded)
                {
                    foreach (var error in linkResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View(nameof(Login));
                }
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["SuccessMessage"] = "Account linked and successfully logged in.";
            return LocalRedirect(returnUrl);
        }

        TempData["ErrorMessage"] = "Email claim not received from: " + info.LoginProvider;
        return RedirectToAction(nameof(Login), new { returnUrl });
    }

    [Authorize]
    public async Task<IActionResult> Security()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var isEnabled = await _accountService.IsTwoFactorEnabledAsync(userId);
        if (isEnabled)
        {
            return RedirectToAction(nameof(TwoFactorEnabled));
        }

        return RedirectToAction(nameof(Setup2FA));
    }

    [Authorize]
    public async Task<IActionResult> TwoFactorEnabled()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var isEnabled = await _accountService.IsTwoFactorEnabledAsync(userId);
        if (!isEnabled)
        {
            return RedirectToAction(nameof(Setup2FA));
        }

        return View();
    }

    // ── CHANGE PASSWORD ─────────────────────────────────────────────────────
    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _accountService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Your password has been changed successfully.";
            return RedirectToAction("Settings", "Citizen");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(dto);
    }

    // ── LOGIN ACTIVITY ──────────────────────────────────────────────────────
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> LoginActivity()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var activities = await _accountService.GetLoginActivityAsync(userId);
        return View(activities);
    }
}
