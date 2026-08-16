using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.WebApp.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApp.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthService _authService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAuthService authService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserName);

        if (user is null || !user.EmailConfirmed || await _userManager.IsLockedOutAsync(user))
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials or inactive account.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        return RedirectToRoleHome(role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> ActivateAccount(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return BadRequest();
        }

        try
        {
            await _authService.ConfirmAccountAsync(userId, token);
            ViewBag.Message = "Your account has been activated. You can now log in.";
        }
        catch (DomainException ex)
        {
            ViewBag.Message = ex.Message;
        }

        return View();
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _authService.RequestPasswordResetAsync(model.Email);

        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToRoleHome(string role) => role switch
    {
        "Administrator" => RedirectToAction("Dashboard", "Admin"),
        "Cashier" => RedirectToAction("Home", "Cashier"),
        "Client" => RedirectToAction("Home", "Client"),
        "Commerce" => RedirectToAction("Home", "Commerce"),
        _ => RedirectToAction(nameof(Login))
    };
}