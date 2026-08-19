using ArtemisBankingPro.Application.DTOs.Users;
using ArtemisBankingPro.Application.Services.Interfaces;
using ArtemisBankingPro.Domain.Exceptions;
using ArtemisBankingPro.WebApp.Filters;
using ArtemisBankingPro.WebApp.ViewModels.Users;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApp.Controllers;

[RoleAuthorize("Administrator")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index(string? role, int pageNumber = 1)
    {
        const int pageSize = 10;

        var result = await _userService.GetUsersAsync(role, pageNumber, pageSize);

        var viewModel = new UserListViewModel
        {
            RoleFilter = role,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = result.TotalPages,
            Users = result.Items.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                Cedula = u.Cedula,
                Role = u.Role,
                IsActive = u.IsActive
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateUserViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _userService.CreateUserAsync(new CreateUserDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Cedula = model.Cedula,
                Email = model.Email,
                UserName = model.UserName,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                Role = model.Role,
                InitialAmount = model.InitialAmount
            });

            TempData["SuccessMessage"] = "User created successfully. An activation email has been sent.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return View(new EditUserViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _userService.UpdateUserAsync(new UpdateUserDto
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email
            });

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id, bool isActive)
    {
        var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

        try
        {
            await _userService.ChangeUserStatusAsync(id, requestingUserId, isActive);
            TempData["SuccessMessage"] = "User status updated successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}